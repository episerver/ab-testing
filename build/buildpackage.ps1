
#$PackageDir = 'C:\Users\ansud\Source\Repos\episerver.social\Comments\DeploymentModeration' 
$PackageDir = $args[0] 

#$SolutionDir = 'C:\Users\ansud\Source\Repos\episerver.social\Comments\'
$SolutionDir = $args[1]

Set-Location $PackageDir\

$assemblyVersionFile = "AssemblyVersionAuto.cs"

$match = (Select-String -Path $SolutionDir$assemblyVersionFile -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

Invoke-Expression '..\.nuget\nuget.exe pack Package.nuspec -Version $assemblyVersion'
