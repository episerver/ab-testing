param ([string]$SiteName = "epitesting",
	[string]$DbName = "AlloyEPiServerDB",
	[string]$DbServer = "(local)")

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# Load dependencies
$EPiServerInstallCommon1Path = Resolve-Path "$cwd\resources\EPiServerInstall.Common.1.dll"
Import-Module WebAdministration
Import-Module sqlps -DisableNameChecking
[System.Reflection.Assembly]::LoadFrom($EPiServerInstallCommon1Path) | Import-Module  -DisableNameChecking

function Delete-Site {
    param (
            $SiteName,
            $SitePath
        )

    $site = "IIS:\Sites\Default Web Site\$SiteName"
    $appPoolName = "IIS:\AppPools\$SiteName"

    if(Test-Path ($appPoolName)) {
        "Stopping $appPoolName"
        Stop-WebItem $appPoolName -Passthru
    }

    Function DeleteIfExists($path) {
        if(Test-Path($path)) {
            "Deleting: $path"
            Remove-Item $path -Recurse
        }
    }

    DeleteIfExists $site
    DeleteIfExists $appPoolName
}

function Detach-Database {
    param (
        $DbFile,
        $DbServer
    )
    Invoke-SqlCmd -Query "ALTER DATABASE [$DbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" -ServerInstance $DbServer -ErrorAction SilentlyContinue
    Invoke-SqlCmd -Query "sp_detach_db '$DbName'" -ServerInstance $DbServer -ErrorAction SilentlyContinue
}



$SitePath = Resolve-Path "$cwd\..\samples\EPiServer.Templates.Alloy"

&sqllocaldb stop v11.0

# Detach database
$databasePath = Join-Path $SitePath "App_Data"
Detach-Database $DbName $DbServer
Remove-Item "$databasePath\*" -include *.mdf,*.ldf

# Remove IIS site
Delete-Site $SiteName $SitePath

# Git clean
&git clean -d -x -f

&sqllocaldb start v11.0