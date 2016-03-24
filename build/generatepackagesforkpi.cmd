IF exist "..\KpiDeployment\lib" ( rd "..\KpiDeployment\lib" /s /q )

md "..\KpiDeployment\lib"

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
xcopy "..\artifacts\"%Configuration%"\net451\EPiServer.Marketing.KPI.dll" "..\KpiDeployment\lib\"  /I /F /R /Y

IF exist "..\KpiDeployment\*.nuspec" ( del "..\KpiDeployment\*.nuspec" /s /q )
xcopy "..\src\EPiServer.Marketing.KPI\Package.nuspec" "..\KpiDeployment\"  /I /F /R /Y

rem cd "..\KpiDeployment\" 
rem rename EPiServer.Marketing.Messaging.nuspec Package.nuspec

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "..\KpiDeployment" "..\src\\"

xcopy "..\KpiDeployment"\*nupkg "..\Deployment"  /I /F /R /Y

