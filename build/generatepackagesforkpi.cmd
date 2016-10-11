IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\KpiDeployment"
set ProjectPath="..\src\EPiServer.Marketing.KPI"

IF exist "%PackagePath%" ( rd "%PackagePath%" /s /q )

md "%PackagePath%\lib"

xcopy "..\artifacts\%Configuration%\net45\EPiServer.Marketing.KPI.dll" "%PackagePath%\lib\"  /I /F /R /Y

xcopy "%ProjectPath%\Package.nuspec" "%PackagePath%\"  /I /F /R /Y

md "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\EmbeddedLangFiles"
xcopy "%ProjectPath%\EmbeddedLangFiles\EPiServer_Kpi_EN.xml" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\EmbeddedLangFiles\"  /I /F /R /Y
xcopy "%ProjectPath%\module.config" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\"  /I /F /R /Y

xcopy "..\src\Database"\KPI\*.sql "%PackagePath%\tools\epiupdates\sql"  /I /F /R

md "%PackagePath%\temp"

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildzip.ps1" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi" "%PackagePath%\temp\EPiServer.Marketing.Kpi.zip"
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" Unblock-File "%PackagePath%\temp\EPiServer.Marketing.Kpi.zip"

for /d %%i in ("%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\*") do rd /s /q "%%i"
del /q "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\*.*"

xcopy "%PackagePath%\temp\*.zip" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.Kpi\"  /I /F /R /Y
rd "%PackagePath%\temp" /s /q


"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" ".."

xcopy "%PackagePath%"\*nupkg "..\artifacts"  /I /F /R /Y

rd "%PackagePath%" /s /q
