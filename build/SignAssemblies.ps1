# Signs assemblies

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
