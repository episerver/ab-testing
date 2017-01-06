$PackageDir = $args[0] 
$SolutionDir = $args[1]
$VersionDir = $args[2]

$assemblyInfoPath = Join-Path $VersionDir "AssemblyVersionAuto.cs"
$nugetPath = Join-Path $SolutionDir "build\resources\nuget\nuget.exe"
$nuspecPath = Join-Path $PackageDir "Package.nuspec"

$match = (Select-String -Path $assemblyInfoPath -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

Invoke-Expression "$nugetPath pack $nuspecPath -Version $assemblyVersion -BasePath $PackageDir -OutputDirectory $PackageDir"
