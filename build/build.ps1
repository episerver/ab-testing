param ([string]$configuration = "Release",
    [string]$runTests = "false",
    [string]$pack = "false")

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# TODO: 
# Install runtime dependencies
# Install node dependencies

"Building $configuration"

# TODO: 
# Restore packages
# Build all xprojs

# Get the latest msbuild version
Get-ChildItem "C:\Program Files (x86)\MSBuild\1*" | ForEach-Object {
    $msbuild = "$_\bin\MSBuild.exe"
}

# Build msbuild projects
&"$msbuild" ..\Multivariate\EPiServer.Marketing.Testing.sln -p:Configuration=$configuration -p:Platform="Any CPU" 

# TODO: 
# Build the Client Resources
# Run tests
# Create packages