IF exist "..\MultivariateTestingDeployment\content" ( rd "..\MultivariateTestingDeployment\content" /s /q )

md "..\MultivariateTestingDeployment\content\Views"
md "..\MultivariateTestingDeployment\content\Controllers"
md "..\MultivariateTestingDeployment\content\ApiTesting"
md "..\MultivariateTestingDeployment\content\Models"

xcopy "..\src\EpiServer.Marketing.Testing.TestPages\Views\ApiTesting\*.cshtml" "..\MultivariateTestingDeployment\content\Views\ApiTesting\"  /I /F /R /Y
xcopy "..\src\EpiServer.Marketing.Testing.TestPages\Controllers\*.cs" "..\MultivariateTestingDeployment\content\Controllers\" /I /F /R /Y
xcopy "..\src\EpiServer.Marketing.Testing.TestPages\ApiTesting\*.cs" "..\MultivariateTestingDeployment\content\ApiTesting\"  /I /F /R /Y
xcopy "..\src\EpiServer.Marketing.Testing.TestPages\Models\*.cs" "..\MultivariateTestingDeployment\content\Models\"  /I /F /R /Y


IF exist "..\MultivariateTestingDeployment\*.nuspec" ( del "..\MultivariateTestingDeployment\*.nuspec" /s /q )
xcopy "..\src\EpiServer.Marketing.Testing.TestPages\*.nuspec" "..\MultivariateTestingDeployment\"  /I /F /R /Y

rem cd "..\MultivariateTestingDeployment\" 
"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" powershell -ExecutionPolicy ByPass -File "buildpackage.ps1" "..\MultivariateTestingDeployment" "..\src\\"
cd "..\MultivariateTestingDeployment\"
xcopy "*nupkg" "..\Deployment"  /I /F /R /Y
Pause
