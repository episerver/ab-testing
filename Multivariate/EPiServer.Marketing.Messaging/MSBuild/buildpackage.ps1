
#$SolutionDir = 'C:\Users\ansud\Source\Repos\episerver.messaging\' 
$SolutionDir = $args[0] 

Set-Location $SolutionDir\Deployment\

$assemblyVersionFile = "AssemblyVersionAuto.cs"

echo $SolutionDir$assemblyVersionFile

$match = (Select-String -Path $SolutionDir$assemblyVersionFile -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

Invoke-Expression '..\.nuget\nuget.exe pack EPiServer.Messaging.nuspec -Version $assemblyVersion'
