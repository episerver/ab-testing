@echo off
@echo.
rem ******************************************
rem
rem	Add new translations strings to the _en file and run this tool
rem 	The TranslationSupport.exe file was created from stash project https://stash.ep.se/projects/ST/repos/build-tools/browse
rem
rem ******************************************
cd src\EPiServer.Marketing.Testing.Web\EmbeddedLangFiles
mkdir diffs
@echo  *** Normalizing (keys in order and lower case) and generating diff files
..\..\..\TranslationSupport.exe normalizeanddiff -r=".\\" -d=".\\diffs"
@echo  *** Merging diff files into existing language files
..\..\..\TranslationSupport.exe mergediff -r=".\\" -d=".\\diffs"
@echo  *** Normalizing (puts all keys in order and lower case)
..\..\..\TranslationSupport.exe normalize -r=".\\" -d=".\\diffs"
rmdir /S /Q diffs
cd ..\..\..\
@echo.
@echo on

