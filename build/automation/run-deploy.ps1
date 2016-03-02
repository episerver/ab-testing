Param(
    $SiteName = "episerver-test", 
	$Version = "", 
    $SitePath = "c:\episerver\$SiteName", 
    $SiteZip = "DailySite.zip", 
	[String[]] $Packages = @("EPiServer.Marketing.Messaging", "EPiServer.Marketing.Testing", "EPiServer.Marketing.Testing.TestPages"),     
    [bool]$DeleteSite = $true, 
    [bool]$CreateSite = $true, 
    $NugetFeed = "http://10.99.101.110/guestAuth/app/nuget/v1/FeedService.svc/",
    $DbServer = "(local)", 
    $DbUsername = "Deployer",
    $DbPassword = "aj3YpcmkDVEuLSiL", 
    $DbSiteUser = "episerver-site",
    $DbSitePassword = "PLeg3BiD9-uJMkpY",
	$LicenseFile = "C:\LicenseFiles\EPiServer 7\License.config"
)

$SiteName = $SiteName -replace "\W+", "-"
$SitePath = "c:\episerver\$SiteName"
"SiteName: $SiteName"

$tmpFolder = [System.IO.Path]::GetTempPath() + [guid]::NewGuid().ToString()
$tmpPackageFolder = "$tmpFolder\$SiteZip.$Version"

$FrameworkDir = $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory())
Set-Alias aspnet_regsql (Join-Path $FrameworkDir "aspnet_regsql.exe")

#http://www.iis.net/learn/manage/powershell/powershell-snap-in-creating-web-sites-web-applications-virtual-directories-and-application-pools
Import-Module WebAdministration
Import-Module sqlps -DisableNameChecking
[System.Reflection.Assembly]::LoadWithPartialName("EPiServerInstall.Common.1") | Import-Module  -DisableNameChecking

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
    DeleteIfExists $SitePath
}

function Create-Site {
    param (
            $SiteName,
            $SitePath
        )


    $site = "IIS:\Sites\Default Web Site\$SiteName"
    $appPoolName = "IIS:\AppPools\$SiteName"

    if(!(Test-Path($SitePath))) {
        "Creating folder: $SitePath"
        md $SitePath
        "Creating folder: $SitePath\AppData"
        md "$Sitepath\AppData"
    }

    if(!(Test-Path($site))) {
        "Creating site: $site"
        New-Item $site -type Application -physicalPath "$SitePath"

        if(!(Test-Path($appPoolName)))
        {
            "Creating Application pool: $appPoolName"
            $appPool = New-Item ($appPoolName)
            $appPool.managedRuntimeVersion = "v4.0"
            $appPool | Set-Item
        }

        #Set the app pool on the site
        "Set $appPoolName as the application pool for $site"
        Set-ItemProperty $site -name applicationPool -value $SiteName
    } else {
        "Site $site already exists"
    }
}

function Deploy-Nuget {
    param (
            $PackageName,
			$PackageVersion,
            $SitePath,
            $NugetFeed
        )

    #Install the nuget package into the $SitePath
    nuget install $PackageName -Version $PackageVersion -Prerelease -OutputDirectory $SitePath -Source $NugetFeed -NoCache 
}

function Deploy-Zip {
    param (
            $ZipName,
            $SitePath
        )

	[System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem')
	"Extracting $ZipName to $SitePath"
    [System.IO.Compression.ZipFile]::ExtractToDirectory($ZipName, $SitePath)
}

function Attach-Database {
    param (
        $DbFile,
        $SiteName,
        $DbServer,
        $DbSiteUser,
        $DbSitePassword
    )
    $DbName = "daily-" + $SiteName

    "Create database [$DbName], attaching $DbFile, granting access for '$DbSiteUser'"
    Invoke-SqlCmd -Query "CREATE database [$DbName] ON (FILENAME = '$DbFile') FOR ATTACH_REBUILD_LOG" -ServerInstance $DbServer
    Invoke-SqlCmd -Query "EXEC sp_addlogin @loginame='$DbSiteUser', @passwd='$DbSitePassword', @defdb='$DbName'" -ServerInstance $DbServer
    Invoke-SqlCmd -Query "USE [$DbName]; EXEC sp_adduser @loginame='$DbSiteUser'" -ServerInstance $DbServer
    Invoke-SqlCmd -Query "USE [$DbName]; EXEC sp_addrolemember N'db_owner', N'$DbSiteUser'" -ServerInstance $DbServer
    Invoke-SqlCmd -Query "EXEC sp_addsrvrolemember '$DbSiteUser', 'dbcreator'" -ServerInstance $DbServer
}

function Detach-Database {
    param (
        $SiteName,
        $DbServer,
        $DbSiteUser
    )
    $DbName = "daily-" + $SiteName

    "Setting database [$DbName] single user, revoking access for user '$DbSiteUser', detaching database"
    Invoke-SqlCmd -Query "ALTER DATABASE [$DbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE" -ServerInstance $DbServer -ErrorAction SilentlyContinue
    Invoke-SqlCmd -Query "USE [$DbName]; EXEC sp_revokedbaccess @name_in_db='$DbSiteUser'" -ServerInstance $DbServer -ErrorAction SilentlyContinue
    Invoke-SqlCmd -Query "EXEC sp_droplogin @loginame='$DbSiteUser'" -ServerInstance $DbServer -ErrorAction SilentlyContinue
    Invoke-SqlCmd -Query "sp_detach_db '$DbName'" -ServerInstance $DbServer -ErrorAction SilentlyContinue
}

function Transform-Config {
    param (
        $TmpPackageFolder,
        $SitePath,
        $DbServer,
        $SiteName,
        $DbSiteUser,
        $DbSitePassword
    )

    "Transform config files"
    Update-EPiXmlFile -TargetFilePath "$SitePath\connectionStrings.config" -ModificationFilePath "$TmpPackageFolder\Setup\connectionStrings.transform" -Replaces "{DbServer}=$DbServer;{DbDatabase}=$SiteName;{DbUserName}=$DbSiteUser;{DbPassword}=$DbSitePassword;"
    Update-EPiXmlFile -TargetFilePath "$SitePath\EPiServerFramework.config" -ModificationFilePath "$TmpPackageFolder\Setup\EPiServerFramework.transform" -Replaces "{basePath}=$SitePath\appData;"

    Update-EPiXmlFile -TargetFilePath "$SitePath\Web.config" -ModificationFilePath "$TmpPackageFolder\Setup\Web.transform" -Replaces "{siteName}=$SiteName;"
}

if ($DeleteSite -eq $true) {
    #Detach-Database $SiteName $DbServer $DbSiteUser
	Delete-Site $SiteName $SitePath
}

if ($CreateSite -eq $true) {
    Create-Site  $SiteName $SitePath
	Deploy-Zip  $SiteZip $SitePath
    
	#$DbPath = $SitePath + "\App_Data\AlloyEPiServerDB.mdf"    
	#Attach-Database $DbPath $SiteName $DbServer $DbSiteUser $DbSitePassword	
	
	#Transform-Config $tmpPackageFolder $SitePath $DbServer $SiteName $DbSiteUser $DbSitePassword
	
	Foreach ($package in $Packages)
	{
		Deploy-Nuget $package $Version $SitePath $NugetFeed
	}
	
    Copy-Item $LicenseFile $SitePath

	if(Test-Path($tmpFolder)) {
		"Remove temp folder $tmpFolder"
		Remove-Item $tmpFolder -Recurse
	}
}