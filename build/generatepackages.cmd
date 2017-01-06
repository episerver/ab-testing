IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\Deployment"
set ProjectPath="..\src\EPiServer.Marketing.Testing.Web"

IF exist "%PackagePath%\" ( rd "%PackagePath%\" /s /q )

md "%PackagePath%\lib"

xcopy "..\artifacts\%Configuration%\net45\EPiServer.Marketing.Testing.Web.dll" "%PackagePath%\lib\"  /I /F /R /Y
xcopy "..\artifacts\%Configuration%\net45\EPiServer.Marketing.Testing.DAL.dll" "%PackagePath%\lib\"  /I /F /R /Y
xcopy "..\artifacts\%Configuration%\net45\EPiServer.Marketing.Testing.Core.dll" "%PackagePath%\lib\"  /I /F /R /Y
rem xcopy "..\artifacts\Release\net45\EPiServer.Marketing.Testing.Model.dll" "%PackagePath%\lib\"  /I /F /R /Y

xcopy "%ProjectPath%\*.nuspec" "%PackagePath%\"  /I /F /R /Y

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ClientResources"
xcopy "%ProjectPath%\ClientResources"\* "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ClientResources\"  /S /I /F /R /Y

xcopy "%ProjectPath%\module.config" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing"  /I /F /R /Y

xcopy "%ProjectPath%\web.config.install.xdt" "%PackagePath%\content"  /I /F /R /Y

xcopy "..\src\Database"\Testing\*.sql "%PackagePath%\tools\epiupdates\sql"  /I /F /R 

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\EmbeddedLangFiles"
xcopy "%ProjectPath%\EmbeddedLangFiles\EPiServer_Testing_EN.xml" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\EmbeddedLangFiles\"  /I /F /R /Y

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Scripts"
xcopy "%ProjectPath%\Scripts" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Scripts"  /I /F /R /Y /S

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Images"
xcopy "%ProjectPath%\Images" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Images"  /I /F /R /Y /S

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ClientResources\Admin"
xcopy "%ProjectPath%\Config\AdminConfig.aspx" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Admin\"  /I /F /R /Y

md "%PackagePath%\temp"

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildzip.ps1" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing" "%PackagePath%\temp\EPiServer.Marketing.Testing.zip"
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" Unblock-File "%PackagePath%\temp\EPiServer.Marketing.Testing.zip"

for /d %%i in ("%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\*") do rd /s /q "%%i"
del /q "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\*.*"

xcopy "%PackagePath%\temp\*.zip" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\"  /I /F /R /Y
rd "%PackagePath%\temp" /s /q

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" ".." "%ProjectPath%"

xcopy "%PackagePath%\*.nupkg" "..\artifacts" /I /F /R /Y

rd "%PackagePath%\" /s /q