$deployDir = $args[0] 
$projectDir = $args[1]
$dependency = $args[2]

$artifactsDir = "..\artifacts"
$srcDir = "..\src\"
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

		# bit of a hack, since 9.24.1 is the last version before 10, this needs an extra bump
		if ($dep.equals("EPiServer.Commerce"))
		{
			$maxVersionLimit += 1
		}
		
		$dependencyName = $dep.Replace(".", "")
		$nugetVersions = $nugetVersions + $dependencyName + "MinVersion=" + $minVersion + ";" + $dependencyName + "MaxLimit=" + $maxVersionLimit + ";"
	}

	# need to wrap entire string in quotes for nuget to be happy
	$nugetVersions = '"' + $nugetVersions + '"'
}

$match = (Select-String -Path $assemblyInfoPath -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

# go into dir where everything is to run nuget command from there so that a proj or spec file doesn't need to be specified.  Apparently, this is the only way 
# to pass in multiple versions
cd $deployDir
if ($nugetVersions)
{
	..\..\..\build\resources\nuget\nuget.exe pack -Build -Properties $nugetVersions -Version $assemblyVersion -Exclude *.xproj
}
else # if there are no dependency versions to set, then dont pass in null as it blows nuget up
{
	..\..\..\build\resources\nuget\nuget.exe pack -Build -Version $assemblyVersion -Exclude *.xproj
}
cd "..\..\..\build"
