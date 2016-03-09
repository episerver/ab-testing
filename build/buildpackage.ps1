$PackageDir = $args[0] 
$SolutionDir = $args[1]

Set-Location $PackageDir\

$assemblyVersionFile = "AssemblyVersionAuto.cs"

$match = (Select-String -Path $SolutionDir$assemblyVersionFile -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

Invoke-Expression $SolutionDir'\..\build\resources\nuget\nuget.exe pack Package.nuspec -Version $assemblyVersion'
