param ([string]$configuration = "Debug",
    [string]$clean = "false",
	[string]$DbName = "AlloyEPiServerDB",
	[string]$DbServer = "(localdb)\v11.0")

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

# Load dependencies
Import-Module sqlps -DisableNameChecking
$EPiServerInstallCommon1Path = Resolve-Path "$cwd\resources\EPiServerInstall.Common.1.dll"
[System.Reflection.Assembly]::LoadFrom($EPiServerInstallCommon1Path) | Import-Module  -DisableNameChecking


function Attach-Database {
    param (
        $DbFile,
        $DbName,
        $DbServer
    )
    Invoke-SqlCmd -Query "CREATE database [$DbName] ON (FILENAME = '$DbFile') FOR ATTACH_REBUILD_LOG" -ServerInstance $DbServer
}

function Detach-Database {
    param (
        $DbFile,
        $DbServer
    )
    Invoke-SqlCmd -Query "ALTER DATABASE [$DbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" -ServerInstance $DbServer -ErrorAction SilentlyContinue
    Invoke-SqlCmd -Query "sp_detach_db '$DbName'" -ServerInstance $DbServer -ErrorAction SilentlyContinue
}

function Execute-Sql {
    param (
        $SqlScript,
        $DbName,
        $DbServer
    )
    Invoke-Sqlcmd -InputFile $SqlScript -ServerInstance $DbServer -Database $DbName
}


function Transform-Config {
    param (
        $XmlUpdatePath,
        $SitePath
    )

    Update-EPiXmlFile -TargetFilePath "$SitePath\Web.config" -ModificationFilePath $XmlUpdatePath -Namespaces "d=urn:schemas-microsoft-com:asm.v1"
}


$SitePath = Resolve-Path "$cwd\..\samples\EPiServer.Templates.Alloy"

# Clean
if([System.Convert]::ToBoolean($clean) -eq $true) {
    &"$cwd\clean.ps1"
}

# Build 
&"$cwd\build.ps1" $configuration

# Setup database
$databasePath = Join-Path $SitePath "App_Data"
Detach-Database $DbName $DbServer
Remove-Item "$databasePath\*" -include *.mdf,*.ldf

$databaseFilePath = "$databasePath\AlloyEPiServerDB.mdf"
Copy-Item "$cwd\resources\AlloyEPiServerDB.mdf" $databaseFilePath -force

Attach-Database $databaseFilePath $DbName $DbServer

# Create database structure
$sqlScriptsPath = Resolve-Path "$cwd\..\src\Database"
$sqlScript = "$cwd\SqlScript.sql"
if (Test-Path $sqlScript) {
	Remove-Item $sqlScript
}

foreach ($script in Get-ChildItem $sqlScriptsPath -Filter '*.sql') {
	Add-Content -Path $sqlScript -Value (Get-Content $script.FullName)			
}
if (Test-Path  $sqlScript) {
	"Executing package SQL scripts..."
	Execute-Sql $sqlScript $DbName $DbServer
	Remove-Item $sqlScript
}

Detach-Database $DbName $DbServer

# TODO:
# Setup virtual paths for modules
$xmlUpdatePath = Resolve-Path "$cwd\resources\AlloyDevelopmentConfig.xmlupdate"
Transform-Config  $xmlUpdatePath $SitePath