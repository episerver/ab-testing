param ([string]$configuration = "Release",
    [string]$runTests = "false",
	[string]$jsreporter = "",
	[string]$generateDoc = "false"
	)

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd


# Install runtime dependencies
$ENV:Path = "$cwd;" + $ENV:Path

# Install runtime dependencies
#dnvm install "1.0.0-rc1-update2" -runtime CLR -arch x86 -alias default
#dnvm use default

# Install node dependencies
#pushd ..
#&"$cwd\npm.cmd" install --silent
#if ($lastexitcode -eq 1) {
#    Write-Host "Node dependencies install failed" -foreground "red"
#    exit $lastexitcode
#}
#pushd $cwd

# Restore packages
#dnu restore ..\ --quiet
#if ($lastexitcode -eq 1) {
#    Write-Host "RESTORE failed" -foreground "red"
#    exit $lastexitcode
#}

&"dotnet" restore ..\EPiServer.Marketing.Testing.sln --packages ..\packages


"Building $configuration"

# Get the latest msbuild version, check if vs2017 is installed as they moved the location of msbuild
$msbuild = (gci -Path "C:\Program Files (x86)\Microsoft Visual Studio" -recurse -filter "msbuild.exe" -file -ErrorAction SilentlyContinue | Where-Object {-not($_.FullName -match "amd64")})[0].FullName


# Build msbuild projects
&"$msbuild" ..\EPiServer.Marketing.Testing.sln /p:OutDir=$cwd\..\artifacts /p:Configuration=$configuration /p:Platform="Any CPU"

if ($lastexitcode -eq 1) {
    Write-Host "BUILD failed" -foreground "red"
    exit $lastexitcode
}


if ($lastexitcode -eq 1) {
    Write-Host "BUILD failed" -foreground "red"
    exit $lastexitcode
}


# Generate Sandcastle Documentation, By default only happens on build machine.
if([System.Convert]::ToBoolean($generateDoc) -eq $true) {
	&"$msbuild" /p:Configuration=Release ..\Documentation\KPI\Kpi.shfbproj
	&"$msbuild" /p:Configuration=Release ..\Documentation\\KPI.Commerce\Kpicommerce.shfbproj
	#&"$msbuild" /p:Configuration=Release ..\Documentation\Messaging\Messaging.shfbproj
	&"$msbuild" /p:Configuration=Release ..\Documentation\Testing\Testing.shfbproj
}
# TODO: 
# Build the Client Resources

# Run tests
if([System.Convert]::ToBoolean($runTests) -eq $true) {
    &"$cwd\test.ps1" $configuration $jsreporter
}