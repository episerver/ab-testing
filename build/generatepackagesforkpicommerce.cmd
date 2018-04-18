IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\KpiCommerceDeployment"
set ProjectPath="..\src\EPiServer.Marketing.KPI.Commerce"

IF exist "%PackagePath%" ( rd "%PackagePath%" /s /q )

md "%PackagePath%\lib"
md "%PackagePath%\lib\net461"


xcopy "..\artifacts\EPiServer.Marketing.KPI.Commerce.dll" "%PackagePath%\lib\net461\"  /I /F /R /Y

xcopy "%ProjectPath%\Package.nuspec" "%PackagePath%\"  /I /F /R /Y

xcopy "%ProjectPath%\Config\CommerceKpiConfig.aspx" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce\Admin\"  /I /F /R /Y

xcopy "%ProjectPath%\module.config" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce"  /I /F /R /Y

xcopy "%ProjectPath%\web.config.transform" "%PackagePath%\content"  /I /F /R /Y

md "%PackagePath%\temp"

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildzip.ps1" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce" "%PackagePath%\temp\EPiServer.Marketing.KPI.Commerce.zip"
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" Unblock-File "%PackagePath%\temp\EPiServer.Marketing.KPI.Commerce.zip"

for /d %%i in ("%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce\*") do rd /s /q "%%i"
del /q "%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce\*.*"

xcopy "%PackagePath%\temp\*.zip" "%PackagePath%\content\modules\_protected\EPiServer.Marketing.KPI.Commerce\"  /I /F /R /Y
rd "%PackagePath%\temp" /s /q

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" "%ProjectPath%"

xcopy "%PackagePath%\*.nupkg" "..\artifacts" /I /F /R /Y

rd "%PackagePath%" /s /q
