param ([string]$configuration = "Release",
    [string]$pack = "false",
	[string]$packageVersion = ""
	)

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# Create packages
if([System.Convert]::ToBoolean($pack) -eq $true) {
	# make sure that we have some package version
	if (!$packageVersion) {
		$match = (Select-String -Path $cwd\..\AssemblyVersionAuto.cs -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
		$packageVersion = $match.Groups[1].Value
	}
	
    &"$cwd\pack.ps1" $configuration $pack $packageVersion	
}