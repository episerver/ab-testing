param ([string]$configuration = "Release",
	   [string]$runtestmode = "")
#Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

Write-Host "Run xunit tests"
Write-Host $configuration
Write-Host $teamcity

Write-Host "InvocationName:" $MyInvocation.InvocationName
Write-Host "Path:" $MyInvocation.MyCommand.Path

#find console test runner
Set-Location "$cwd\..\packages"
$runner = Get-ChildItem -Recurse -Filter 'xunit.console.exe'

Set-Location ..\
#find all tests dlls
foreach ($item in Get-ChildItem -Recurse -Filter '*.tests.dll' | Where-Object {$_.FullName -like '*bin*'})
{
    &$runner.FullName $item.FullName $runtestmode
	if ($lastexitcode -eq 1) {
		Write-Host "TEST failed" -foreground "red"
		exit $lastexitcode
	}	
}