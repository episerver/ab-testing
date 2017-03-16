param ([string]$configuration = "Release",
	   [string]$jsreporter = "spec")
#Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

Write-Host "Run tests"
Write-Host $configuration
Write-Host $teamcity

$ENV:Path = "$cwd\;" + $ENV:Path

# Install runtime dependencies
dnvm install "1.0.0-rc1-update2" -runtime CLR -arch x86 -alias default
dnvm use default

# Install node dependencies
pushd ..
&"$cwd\npm.cmd" install --silent
if ($lastexitcode -eq 1) {
    Write-Host "Node dependencies install failed" -foreground "red"
    exit $lastexitcode
}
pushd $cwd

# Run xUnit tests for all test projects
$failBuild = $false
Get-ChildItem "$cwd\..\test" -Filter "*Test" -Exclude "*ClientTest" | ForEach-Object {
	pushd "$_"
    dnx --configuration $configuration test
    if ($lastexitcode -eq 1) {
		$failBuild = $true
        popd
        Write-Host "TEST failed" -foreground "red"   
    }
    popd
}

# Fail the build if any of the tests failed.
if ($failBuild -eq $true){
	Write-Host "One or more Unit Tests failed." -foreground "red"
	exit $lastexitcode
}

# Run JS tests
&"$cwd\npm.cmd" run test  -- --reporter=$jsreporter
if ($lastexitcode -eq 1) {
    Write-Host "Running JS tests failed" -foreground "red"
    exit $lastexitcode
}