using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Text;
using System.Text.Json.Serialization;

namespace LocalizationDownloader
{
    internal class Program
    {
        /// <summary>
        /// Created 2012-05-24
        /// Updated 2022-08-17 (downloadUrl function updated to async call)
        /// 
        /// Usage LocalizationDownloader <url> <output folder>
        /// Where:
        ///     url - your Google Sheet CSV publish link
        ///     output folder - folder to save result json files
        /// </summary>        
        /// <returns>
        /// Status codes:
        ///     0 - successfull
        ///     1 - done with warnings (duplicate names or save file errors for some languages)
        ///     100 - incorrect input parameters
        ///     101 - downloaded empty content by specified url
        ///     102 - downloaded csv empty
        ///     103 - unable to find language display names (variable DisplayLanguageName) from csv
        ///     104 - unable to find variables column from csv
        ///     999 - unexpected exception
        /// </returns>
        static async Task<int> Main(string[] args)
        {
            int exitCode = 0;
            try
            {
                if (args != null && args.Length >= 2)
                {
                    string url = args[0];                    
                    string outputFolder = args[1];
                    string csv = await downloadUrl(url);
                    if (!String.IsNullOrEmpty(csv))
                    {
                        if (!Directory.Exists(outputFolder))
                        {
                            Directory.CreateDirectory(outputFolder);
                            Console.WriteLine($"Output folder created {outputFolder}");
                        }
                        List<List<string>> lines = readCSVFile(csv);
                        if (lines.Count > 0)
                        {
                            Console.WriteLine($"Read {lines.Count} lines");
                            List<string> languageNames = getLanguageNames(lines);
                            if (languageNames.Count > 0)
                            {
                                Console.WriteLine($"Found {languageNames.Count} languages: {string.Join(", ", languageNames)}");
                                int varindex = getValueColumnIndex(lines, "Variables", "Variables");
                                if (varindex >= 0)
                                {
                                    int warnings = 0;
                                    foreach (string language in languageNames)
                                    {
                                        int valueindex = getValueColumnIndex(lines, "DisplayLanguageName", language);
                                        if (valueindex > 0)
                                        {
                                            Dictionary<string, string> items = new Dictionary<string, string>();
                                            for (int i = 0; i < lines.Count; i++)
                                            {
                                                string Name = lines[i][varindex];
                                                string Value = lines[i][valueindex];
                                                if (String.IsNullOrEmpty(Name) == false)
                                                {
                                                    if (!items.ContainsKey(Name))
                                                        items.Add(Name, Value);
                                                    else
                                                    {
                                                        Console.WriteLine($"Variable {Name} already exists with different value");
                                                        warnings++;
                                                    }
                                                }
                                             }

                                            if (items.ContainsKey("LanguageFile"))
                                            {
                                                string outputFile = Path.Combine(outputFolder, $"{items["LanguageFile"]}.json");
                                                if (File.Exists(outputFile))
                                                    Console.WriteLine($"File already exists {outputFile}. Overwriting...");
                                                try
                                                {
                                                    File.WriteAllText(outputFile, JsonConvert.SerializeObject(items, Formatting.Indented));
                                                }
                                                catch (Exception ex)
                                                { 
                                                    Console.WriteLine($"Error saving file {outputFile}: {ex.Message}");
                                                    warnings++;
                                                }
                                            }
                                        }
                                        else
                                            Console.Write($"Unable to find required column 'DisplayLanguageName' for language {language}");
                                    }
                                    if (warnings > 0)
                                    {
                                        Console.WriteLine($"{warnings} warnings occurred during execution");
                                        exitCode = 1;
                                    }
                                }
                                else
                                {
                                    Console.Write("Unable to find required column 'Variables'");
                                    return 104;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Unable to get language names from CSV");
                                exitCode = 103;
                            }
                        }
                        else
                        {
                            Console.WriteLine("CSV contains no data");
                            exitCode = 102;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Downloaded empty content");
                        exitCode = 101;
                    }
                }
                else
                {
                    Console.WriteLine("Incorrect input params");
                    exitCode = 100;
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                exitCode = 999;
            }            
            return exitCode;
        }

        /// <summary>
        /// Download csv string from url
        /// </summary>
        static async Task<string> downloadUrl(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (HttpResponseMessage response = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = response.Content)
                        {
                            return await content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Url download error: {ex.Message}");
                return String.Empty;
            }
        }

        /// <summary>
        /// Read string as csv structure
        /// </summary>
        static List<List<string>> readCSVFile(string csv)
        {
            List<List<string>> result = new List<List<string>>();
            using (TextFieldParser parser = new TextFieldParser(new MemoryStream(Encoding.UTF8.GetBytes(csv))))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[]? fields = parser.ReadFields();
                    if (fields != null)
                        result.Add(fields.ToList());
                }
            }

            return result;
        }

        /// <summary>
        /// Get list of language names from csv structure
        /// </summary>
        static List<string> getLanguageNames(List<List<string>> lines)
        {
            try
            {
                List<string>? languageLine = lines.FirstOrDefault(x => x.Contains("DisplayLanguageName"));
                return (languageLine != null)
                   ? languageLine.Where(x => !String.IsNullOrEmpty(x) && x != "DisplayLanguageName").ToList()
                    : new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get language names error: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Get column index by column name and column value from csv structure
        /// </summary>
        static int getValueColumnIndex(List<List<string>> lines, string columnName, string columnValue)
        {
            List<string>? column = lines.FirstOrDefault(x => x.Contains(columnName));
            return (column != null) ? column.IndexOf(columnValue) : -1;
        }
        
    }
}