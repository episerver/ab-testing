$deployDir = $args[0] 
$projectDir = $args[1]
#$dbSchema = $args[2]
$dependency = $args[2]
#$assemblies = $args[4]

$artifactsDir = "..\artifacts"

#if (Test-Path $deployDir)
#{
#    Remove-Item $deployDir -Recurse
#}

#new-item $deployDir -itemtype directory
#new-item $deployDir\lib -itemtype directory
#new-item $deployDir\tools\epiupdates\sql -itemtype directory

$srcDir = "..\src\"
#$sqlPath = $srcDir + "Database\" + $dbSchema + "\*.sql"

#$projectDir = $srcDir + $projectName + "\"
$jsonPath = $projectDir + "\project.json"
$assemblyInfoPath = $projectDir + "\AssemblyVersionAuto.cs"

# loop over dependencies to get the versions for each one
if ($dependency)
{
$depends = $dependency.Split("*")

$myJson = Get-Content -Raw -Path $jsonPath | ConvertFrom-Json
$nugetVersions = ""

ForEach($dep in $depends) 
{
	$minVersion = $myJson.dependencies.$dep
	$maxVersionLimit = [int]$minVersion.Split('.')[0] + 1

	$dependencyName = $dep.Replace(".", "")
	$nugetVersions = $nugetVersions + $dependencyName + "MinVersion=" + $minVersion + ";" + $dependencyName + "MaxLimit=" + $maxVersionLimit + ";"
}

$nugetVersions = '"' + $nugetVersions + '"'
$nugetVersions
}
$match = (Select-String -Path $assemblyInfoPath -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

#$nuspecPath = $projectDir + "Package.nuspec"
#$projectPath = $projectDir + "" + $projectName + ".xproj"

# loop over this for multiple assemblies
#$assemblies = $assemblies.Split(";")
#ForEach($assembly in $assemblies) 
#{ 
#    $path = "$artifactsDir\Release\net45\" + $assembly + ".dll"
#    Copy-Item -Path $path -Destination $deployDir\lib 
#}

#Copy-Item -Path $nuspecPath -Destination $deployDir
#Copy-Item -Path $projectPath -Destination $deployDir
#Copy-Item -Path $sqlPath -Destination $deployDir\tools\epiupdates\sql -Recurse -Force

cd $deployDir
if ($nugetVersions)
{
	..\..\..\build\resources\nuget\nuget.exe pack -Build -Properties $nugetVersions -Version $assemblyVersion -Exclude *.xproj
}
else
{
	..\..\..\build\resources\nuget\nuget.exe pack -Build -Version $assemblyVersion -Exclude *.xproj
}
cd "..\..\..\build"

#Copy-Item  $deployDir\*.nupkg $artifactsDir
#Remove-Item $deployDir -Recurse