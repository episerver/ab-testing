IF exist "..\MessagingDeployment\lib" ( rd "..\MessagingDeployment\lib" /s /q )

md "..\MessagingDeployment\lib"

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
xcopy "..\artifacts\"%Configuration%"\net45\EPiServer.Marketing.Messaging.dll" "..\MessagingDeployment\lib\"  /I /F /R /Y

IF exist "..\MessagingDeployment\*.nuspec" ( del "..\MessagingDeployment\*.nuspec" /s /q )
xcopy "..\src\EPiServer.Marketing.Messaging\Package.nuspec" "..\MessagingDeployment\"  /I /F /R /Y

rem cd "..\MessagingDeployment\" 
rem rename EPiServer.Marketing.Messaging.nuspec Package.nuspec

"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "..\MessagingDeployment" "..\src\\"

xcopy "..\MessagingDeployment"\*nupkg "..\Deployment"  /I /F /R /Y

