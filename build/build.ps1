param ([string]$configuration = "Release",
    [string]$runTests = "false",
	[string]$runtestmode = "",
    [string]$pack = "false")

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# TODO: 
# Install runtime dependencies
# Install node dependencies

"Building $configuration"

# Restore packages
.\resources\nuget\NuGet.exe restore ..\Multivariate\EPiServer.Marketing.Testing.sln -PackagesDirectory ..\packages
# TODO: 
# Restore packages for xprojs
# Build all xprojs

# Get the latest msbuild version
Get-ChildItem "C:\Program Files (x86)\MSBuild\1*" | ForEach-Object {
    $msbuild = "$_\bin\MSBuild.exe"
}

# Build msbuild projects
&"$msbuild" ..\Multivariate\EPiServer.Marketing.Testing.sln /p:Configuration=$configuration /p:Platform="Any CPU\"

# TODO: 
# Build the Client Resources

# Run tests
if([System.Convert]::ToBoolean($runTests) -eq $true) {
    .\test.ps1 $configuration $runtestmode
    pushd .
}

# Create packages
if([System.Convert]::ToBoolean($pack) -eq $true) {
    .\pack.ps1 configuration $configuration
    pushd .
}