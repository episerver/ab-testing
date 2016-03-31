param ([string]$configuration = "Release",
	   [string]$runtestmode = "")
#Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

Write-Host "Run xunit tests"
Write-Host $configuration
Write-Host $teamcity

$ENV:Path = "$cwd\;" + $ENV:Path

# Install runtime dependencies
dnvm update-self
dnvm install "1.0.0-rc1-update1" -runtime CLR -arch x86 -alias default
dnvm use default

# Run tests for all test projects
Get-ChildItem "$cwd\..\test" -Filter "*Test"  | ForEach-Object {
	pushd "$cwd\..\test\$_"
    dnx --configuration $configuration test
    if ($lastexitcode -eq 1) {
        popd
        Write-Host "TEST failed" -foreground "red"
        exit $lastexitcode
    }
    popd
}