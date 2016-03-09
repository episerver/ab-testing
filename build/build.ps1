param ([string]$configuration = "Release",
    [string]$runTests = "false",
	[string]$runtestmode = "",
    [string]$pack = "false",
	[string]$packageVersion = "")

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# TODO: 
# Install runtime dependencies
# Install node dependencies

"Building $configuration"

# Restore packages
&"$cwd\resources\nuget\NuGet.exe" restore ..\Multivariate\EPiServer.Marketing.Testing.sln -PackagesDirectory ..\packages
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
    &"$cwd\test.ps1" $configuration $runtestmode
}

# Create packages
if([System.Convert]::ToBoolean($pack) -eq $true) {
	# make sure that we have some package version
	if (!$packageVersion) {
		$match = (Select-String -Path $cwd\..\Multivariate\AssemblyVersionAuto.cs -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
		$packageVersion = $match.Groups[1].Value
	}
	
    &"$cwd\pack.ps1" $configuration $pack $packageVersion	
}