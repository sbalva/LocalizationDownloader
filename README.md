Command line utility that can be useful for those who use google sheets for application localization.

How to use:
1) Create google sheet based on the following example (green cells are required) https://docs.google.com/spreadsheets/d/1uxhEZ8OYGtUOxzyylf0dEisd9tjCffvFUOfU1t2vwlc
2) Publish working sheet as csv file and copy publishing link (for example https://docs.google.com/spreadsheets/d/e/2PACX-1vT5gYrwNm-lWSecqf_jSakIRzjcNJTNb0xjDaOIjVjqRMGP0vOi2M-rhyOojqtEZpOwibgU_YKy0csI/pub?gid=1977721534&single=true&output=csv)
3) Run utility with the following parameters: publishing link from #2 and output folder
4) Pick the json-files from the output folder and use it in your app
5) Repeat #3 and 4 after google sheet updates

You will get the result json looks like (from example sheet):

{
  "Variables": "English",
  "CodePage": "1033",
  "LanguageFile": "English",
  "DisplayLanguageName": "English",
  "ErrorMessageTitle": "Error",
  "MenuWelcome": "Welcome",
  "YesText": "Yes",
  "NoText": "No",
  "StartText": "Start",
  "NextText": "Next",
  "BackText": "Back",
  "CancelText": "Cancel",
  "OKText": "OK",
  "ResetText": "Reset",
  "RefreshText": "Refresh",
  "SearchText": "Search"
}
