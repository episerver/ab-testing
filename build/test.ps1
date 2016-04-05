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
dnvm install "1.0.0-rc1-update2" -runtime CLR -arch x86 -alias default
dnvm use default

# Install node dependencies
npm install ..
if ($lastexitcode -eq 1) {
    Write-Host "Node dependencies install failed" -foreground "red"
    exit $lastexitcode
}

gulp test 
if ($lastexitcode -eq 1) {
    Write-Host "Running JS tests failed" -foreground "red"
    exit $lastexitcode
}

# Run tests for all test projects
Get-ChildItem "$cwd\..\test" -Filter "*Test" -Exclude "*ClientTest" | ForEach-Object {
	pushd "$_"
    dnx --configuration $configuration test
    if ($lastexitcode -eq 1) {
        popd
        Write-Host "TEST failed" -foreground "red"
        exit $lastexitcode
    }
    popd
}