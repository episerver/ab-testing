IF exist "..\Deployment\lib\" ( rd "..\Deployment\lib" /s /q )
md "..\Deployment\lib"

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
xcopy "..\artifacts\"%Configuration%"\net45\EPiServer.Marketing.Testing.Web.dll" "..\Deployment\lib\"  /I /F /R /Y
xcopy "..\artifacts\"%Configuration%"\net45\EPiServer.Marketing.Testing.DAL.dll" "..\Deployment\lib\"  /I /F /R /Y
xcopy "..\artifacts\"%Configuration%"\net45\EPiServer.Marketing.Testing.Core.dll" "..\Deployment\lib\"  /I /F /R /Y
rem xcopy "..\artifacts\Release\net45\EPiServer.Marketing.Testing.Model.dll" "..\Deployment\lib\"  /I /F /R /Y

IF exist "..\Deployment\*.nuspec" ( del "..\Deployment\*.nuspec" /s /q )
xcopy "..\src\EPiServer.Marketing.Testing.Web\*.nuspec" "..\Deployment\"  /I /F /R /Y

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingAdministration" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingAdministration" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingAdministration"
xcopy "..\src\EPiServer.Marketing.Testing.Web\Views\TestingAdministration"\*.ascx "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingAdministration\"  /I /F /R /Y

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingGadget" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingGadget" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingGadget"
xcopy "..\src\EPiServer.Marketing.Testing.Web\Views\TestingGadget"\*.ascx "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Views\TestingGadget\"  /I /F /R /Y

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Web" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Web" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate"
xcopy "..\src\EPiServer.Marketing.Testing.Web\Web\MultivariateConfiguration.aspx" "..\Deployment\content\modules\_protected\EPiServer.Multivariate"  /I /F /R /Y
xcopy "..\src\EPiServer.Marketing.Testing.Web\module.config" "..\Deployment\content\modules\_protected\EPiServer.Multivariate"  /I /F /R /Y

IF exist "..\Deployment\tools\epiupdates\sql" ( del "..\Deployment\tools\epiupdates\sql" /s /q )
xcopy "..\Database"\*.sql "..\Deployment\tools\epiupdates\sql"  /I /F /R 

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\EmbeddedLangFiles" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\EmbeddedLangFiles" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate\EmbeddedLangFiles"
xcopy "..\src\EPiServer.Marketing.Testing.Web\EmbeddedLangFiles\EPiServer_Testing_EN.xml" "..\Deployment\content\modules\_protected\EPiServer.Multivariate\EmbeddedLangFiles\"  /I /F /R /Y

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Scripts" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Scripts" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Scripts"
xcopy "..\src\EPiServer.Marketing.Testing.Web\Scripts" "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Scripts"  /I /F /R /Y /S

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Images" ( rd "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Images" /s /q )
md "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Images"
xcopy "..\src\EPiServer.Marketing.Testing.Web\Images" "..\Deployment\content\modules\_protected\EPiServer.Multivariate\Images"  /I /F /R /Y /S

IF exist "..\Deployment\temp\" ( rd "..\Deployment\temp" /s /q )
md "..\Deployment\temp"

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildzip.ps1" "..\Deployment\content\modules\_protected\EPiServer.Multivariate" "..\Deployment\temp\EPiServer.Multivariate.zip"
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" Unblock-File "..\Deployment\temp\EPiServer.Multivariate.zip"

for /d %%i in ("..\Deployment\content\modules\_protected\EPiServer.Multivariate\*") do rd /s /q "%%i"
del /q "..\Deployment\content\modules\_protected\EPiServer.Multivariate\*.*"

xcopy "..\Deployment\temp\*.zip" "..\Deployment\content\modules\_protected\EPiServer.Multivariate\"  /I /F /R /Y
rd "..\Deployment\temp" /s /q

IF exist "..\Deployment\content\modules\_protected\EPiServer.Multivariate\module.config" ( del "..\Deployment\content\modules\_protected\EPiServer.Multivariate\module.config" /s /q )
xcopy "..\src\EPiServer.Marketing.Testing.Web\module.config" "..\Deployment\content\modules\_protected\EPiServer.Multivariate\"  /I /F /R /Y

rem "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "..\src\EPiServer.Marketing.Testing.Web\..\..\build\buildpackage.ps1" "..\src\EPiServer.Marketing.Testing.Web\..\Deployment" "..\src\EPiServer.Marketing.Testing.Web\..\\"
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "..\Deployment" "..\src\\" 

rem xcopy "..\Deployment\*.nupkg" "..\artifacts\Release\net45\" /I /F /R /Y
