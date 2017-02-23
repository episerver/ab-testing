IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)

set PackagePath="..\artifacts\%Configuration%\MessagingDeployment"
set ProjectPath="..\src\EPiServer.Marketing.Messaging"

IF exist "%PackagePath%" ( rd "%PackagePath%" /s /q )

md "%PackagePath%\lib"

xcopy "..\artifacts\%Configuration%\net45\EPiServer.Marketing.Messaging.dll" "%PackagePath%\lib\"  /I /F /R /Y

xcopy "%ProjectPath%\Package.nuspec" "%PackagePath%\"  /I /F /R /Y

xcopy "%ProjectPath%\EPiServer.Marketing.Messaging.xproj" "%PackagePath%\"  /I /F /R /Y

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "%PackagePath%" "%ProjectPath%"

xcopy "%PackagePath%"\*nupkg "..\artifacts"  /I /F /R /Y

rd "%PackagePath%" /s /q