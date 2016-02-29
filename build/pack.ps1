param ([string]$configuration = "Release")

#Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

$root = Resolve-Path ".."
$artifactsPath = Resolve-Path "..\artifacts"

if (!(Test-Path $artifactsPath))
{
	New-Item -ItemType directory -Path $artifactsPath
}

# TODO:
# Creating NuGet packages

# Creating daily site package.
# Copying database file to the site folder:

if (Test-Path $artifactsPath\DailySite.zip)
{
	Remove-Item $artifactsPath\DailySite.zip -Force
}

Copy-Item .\resources\AlloyEPiServerDB.mdf ..\samples\EPiServer.Templates.Alloy\App_Data

.\buildzip.ps1 $root\samples\EPiServer.Templates.Alloy $artifactsPath\DailySite.zip
