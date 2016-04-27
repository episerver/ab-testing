IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\TestPagesDeployment"
set ProjectPath="..\samples\EpiServer.Marketing.Testing.TestPages"

IF exist "%PackagePath%" ( rd "%PackagePath%" /s /q )

md "%PackagePath%\content\Views"
md "%PackagePath%\content\Controllers"
md "%PackagePath%\content\ApiTesting"
md "%PackagePath%\content\Models"

xcopy "%ProjectPath%\Views\ApiTesting\*.cshtml" "%PackagePath%\content\Views\ApiTesting\"  /I /F /R /Y
xcopy "%ProjectPath%\Controllers\*.cs" "%PackagePath%\content\Controllers\" /I /F /R /Y
xcopy "%ProjectPath%\ApiTesting\*.cs" "%PackagePath%\content\ApiTesting\"  /I /F /R /Y
xcopy "%ProjectPath%\Models\*.cs" "%PackagePath%\content\Models\"  /I /F /R /Y


xcopy "%ProjectPath%\*.nuspec" "%PackagePath%\"  /I /F /R /Y

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" ".."

xcopy "%PackagePath%\*nupkg" "..\artifacts"  /I /F /R /Y

rd "%PackagePath%" /s /q