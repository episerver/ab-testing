$deployDir = $args[0] 
$projectDir = $args[1]

$artifactsDir = "..\artifacts"
$srcDir = "..\src\"
$jsonPath = $projectDir + "\project.json"
$assemblyInfoPath = $projectDir + "\AssemblyVersionAuto.cs"

if(Test-Path -Path $projectDir\*.csproj){
    [xml]$csProj = Get-Content -Path $projectDir\*.csproj | Out-String
    $nugetVersions = ""

    $nuspecPath = (get-childitem $deployDir\*.nuspec).FullName
    [xml]$nuspec = Get-Content -Path $projectDir\*.nuspec | Out-String
    
    $projectreferencenames = $csProj.Project.ItemGroup.ProjectReference.Include | ForEach-Object { if($_ -ne $null) { (split-path $_).Replace("..\", "") }}
    $defaultpackageincludes = $nuspec.package.metadata.dependencies.dependency.id
    $projectreferencenames = $projectreferencenames | Where-Object { if($_ -ne $null) { -not $defaultpackageincludes.contains($_) }}

    $allprojectcs = $csProj.Project.ItemGroup.ProjectReference.Include | where-object { ($_ -ne $null) -and (-not ((split-path $_).Replace("..\", "")) -in $projectreferencenames) } | ForEach-Object {[xml](Get-Content -Raw -Path "$projectDir\$_")}
    $allprojectcs += $csProj
    $packagereferences = $allprojectcs.Project.ItemGroup | select PackageReference 
    $packagereferences = $packagereferences.PackageReference | Sort-Object -Property Include -Unique

    $nuspec.package.metadata.dependencies.dependency | Where-Object { ($_ -ne $null) -and (-not($_.id.StartsWith("EPiServer.Marketing"))) } | ForEach-Object { $_.ParentNode.RemoveChild($_) }

    foreach($packagedependency in $packagereferences){
        $newdependency = $nuspec.CreateElement("dependency")
        $idattr = $nuspec.CreateAttribute("id")
        $newdependency.Attributes.Append($idattr)
        $newdependency.SetAttribute("id", $packagedependency.Include)

        $versionattr = $nuspec.CreateAttribute("version")
        $newdependency.Attributes.Append($versionattr)

        $minversion = $packagedependency.Version
        $maxVersion = [int]$minVersion.Split('.')[0] + 1
        $newdependency.SetAttribute("version", "[$minVersion,$maxVersion)")

        "Adding dependency "
        $newdependency
        " to $nuspecPath"
    
        $nuspec.SelectSingleNode('/package/metadata/dependencies').AppendChild($newdependency)
    }

    ForEach($dep in $nuspec.package.metadata.dependencies.dependency) 
    {
        $minVersion = "1.0.0"
        if($dep.id.StartsWith("EPiServer.Marketing")){
            $projectversionfile = Get-Content -Raw -Path ("$projectDir\..\{0}\AssemblyVersionAuto.cs" -f $dep.id)
            $partialfile = $projectversionfile.Substring($projectversionfile.IndexOf("AssemblyInformationalVersion(""") + "AssemblyInformationalVersion(""".Length)
            $minVersion = $partialfile.Substring(0, $partialfile.IndexOf(""""))
            $maxVersion = [int]$minVersion.Split('.')[0] + 1
            $dep.version = "[$minVersion,$maxVersion)"
        }
    }

    $nuspec.Save($nuspecPath)
}

$match = (Select-String -Path $assemblyInfoPath -Pattern 'AssemblyInformationalVersion\("(.+)"\)').Matches[0]
$assemblyVersion = $match.Groups[1].Value

# go into dir where everything is to run nuget command from there so that a proj or spec file doesn't need to be specified.  Apparently, this is the only way 
# to pass in multiple versions
cd $deployDir
..\..\..\build\resources\nuget\nuget.exe pack -Build -Version $assemblyVersion

cd "..\..\..\build"
