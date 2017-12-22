IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\Deployment"
set ProjectPath="..\src\EPiServer.Marketing.Testing.Web"

IF exist "%PackagePath%\" ( rd "%PackagePath%\" /s /q )

md "%PackagePath%\lib"
md "%PackagePath%\lib\net461"

xcopy "..\artifacts\EPiServer.Marketing.Testing.Web.dll" "%PackagePath%\lib\net461\"  /I /F /R /Y
xcopy "..\artifacts\EPiServer.Marketing.Testing.DAL.dll" "%PackagePath%\lib\net461\"  /I /F /R /Y
xcopy "..\artifacts\EPiServer.Marketing.Testing.Core.dll" "%PackagePath%\lib\net461\"  /I /F /R /Y

xcopy "%ProjectPath%\*.nuspec" "%PackagePath%\"  /I /F /R /Y

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ClientResources"
xcopy "%ProjectPath%\ClientResources"\* "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ClientResources\"  /S /I /F /R /Y

xcopy "%ProjectPath%\module.config" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing"  /I /F /R /Y

xcopy "%ProjectPath%\web.config.install.xdt" "%PackagePath%\content"  /I /F /R /Y
xcopy "%ProjectPath%\web.config.transform" "%PackagePath%\content"  /I /F /R /Y

xcopy "..\src\Database"\Testing\*.sql "%PackagePath%\tools\epiupdates\sql"  /I /F /R 

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\EmbeddedLangFiles"
xcopy "%ProjectPath%\EmbeddedLangFiles\EPiServer_Testing_EN.xml" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\EmbeddedLangFiles\"  /I /F /R /Y

rem md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Scripts"
rem xcopy "%ProjectPath%\Scripts" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\Scripts"  /I /F /R /Y /S

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

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\abcapture"
xcopy "%ProjectPath%\ClientResources\abcapture\*.*" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Testing\ABCapture\"  /I /F /R /Y


"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" "%ProjectPath%"

xcopy "%PackagePath%\*.nupkg" "..\artifacts" /I /F /R /Y

REM rd "%PackagePath%\" /s /qrd "%PackagePath%\" /s /q