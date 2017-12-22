IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\KpiDeployment"
set ProjectPath="..\src\EPiServer.Marketing.KPI"

IF exist "%PackagePath%" ( rd "%PackagePath%" /s /q )

md "%PackagePath%\lib"
md "%PackagePath%\lib\net461"


xcopy "..\artifacts\EPiServer.Marketing.KPI.dll" "%PackagePath%\lib\net461\"  /I /F /R /Y

xcopy "%ProjectPath%\Package.nuspec" "%PackagePath%\"  /I /F /R /Y

xcopy "..\src\Database"\KPI\*.sql "%PackagePath%\tools\epiupdates\sql"  /I /F /R

rem get versions

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" "%ProjectPath%"

xcopy "%PackagePath%"\*nupkg "..\artifacts"  /I /F /R /Y

rd "%PackagePath%" /s /q
