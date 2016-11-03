param ([string]$configuration = "Release",
    [string]$runTests = "false",
	[string]$jsreporter = "",
    [string]$pack = "false",
	[string]$packageVersion = "",
	[string]$signAssemblies = "false"
	)

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd


# Install runtime dependencies
$ENV:Path = "$cwd;" + $ENV:Path

# Install runtime dependencies
dnvm update-self
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

# Restore packages
dnu restore ..\ --quiet
if ($lastexitcode -eq 1) {
    Write-Host "RESTORE failed" -foreground "red"
    exit $lastexitcode
}

&"$cwd\resources\nuget\NuGet.exe" restore ..\EPiServer.Marketing.Testing.Net45.sln -PackagesDirectory ..\packages

"Building $configuration"
# Build all xprojs
dnu build ..\** --quiet --configuration $configuration --out ..\artifacts
if ($lastexitcode -eq 1) {
    Write-Host "BUILD failed" -foreground "red"
    exit $lastexitcode
}

# Get the latest msbuild version
Get-ChildItem "C:\Program Files (x86)\MSBuild\1*" | ForEach-Object {
    $msbuild = "$_\bin\MSBuild.exe"
}

# Build msbuild projects
&"$msbuild" ..\EPiServer.Marketing.Testing.Net45.sln /p:Configuration=$configuration /p:Platform="Any CPU"


# TODO: 
# Build the Client Resources

# Run tests
if([System.Convert]::ToBoolean($runTests) -eq $true) {
    &"$cwd\test.ps1" $configuration $jsreporter
}


# Signs assemblies
if([System.Convert]::ToBoolean($signAssemblies) -eq $true) {
	$rootDir = Get-Location
	$srcProjects = (Get-ChildItem -Directory -Path (Join-Path ($rootDir) ".\src") -Exclude Database)
	$signError = $false
	$assemblies = @()

	foreach($item in $srcProjects)
	{
		$assemblies = $assemblies + (Get-ChildItem -Recurse -Path (Join-Path ($rootDir) ".\artifacts") -File -Filter ($item.Name + ".dll") -Exclude *Sources.dll)
		$assemblies = $assemblies + (Get-ChildItem -Recurse -Path (Join-Path ($rootDir) ".\src\$($item.Name)") -File -Filter ($item.Name + ".dll") -Exclude *Sources.dll)
		$assemblies = $assemblies + (Get-ChildItem -Recurse -Path (Join-Path ($rootDir) ".\test\$($item.Name)") -File -Filter ($item.Name + ".dll") -Exclude *Sources.dll)
	}

	foreach ($assembly in $assemblies)
	{
	   Write-Host ("Signing " + $assembly.FullName)
	   $LASTEXITCODE = 0
	   &"%WindowsSDKv8.0A_Path%\bin\NETFX 4.0 Tools\sn.exe " -q -Rc  $assembly.FullName "EPiServerProduct"
	   if ($LASTEXITCODE -ne 0)
	   {
		   exit $LASTEXITCODE
	   }
	}
}

# Create packages
if([System.Convert]::ToBoolean($pack) -eq $true) {
	# make sure that we have some package version
	if (!$packageVersion) {
		$match = (Select-String -Path $cwd\..\AssemblyVersionAuto.cs -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
		$packageVersion = $match.Groups[1].Value
	}
	
    &"$cwd\pack.ps1" $configuration $pack $packageVersion	
}