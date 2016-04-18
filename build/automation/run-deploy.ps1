Param(
    $SiteName = "episerver-test", 
	$PackageVersion = "", 	
	$PackageName = "EPiServer.Marketing.Testing.DailySite", 
    $SitePath = "c:\episerver\$SiteName", 
    $SiteZip = "DailySite.zip", 	
    [bool]$DeleteSite = $true, 
    [bool]$CreateSite = $true, 
    $NugetFeed = "http://10.99.101.110/guestAuth/app/nuget/v1/FeedService.svc/;http://nuget.episerver.com/feed/packages.svc/;https://www.nuget.org/api/v2",
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

if($DbSiteUser -eq "") {
    "No site username provided, using '$SiteName'"
    $DbSiteUser = $SiteName;
}

$tmpFolder = [System.IO.Path]::GetTempPath() + [guid]::NewGuid().ToString()
$tmpPackageFolder = "$tmpFolder\$PackageName.$PackageVersion"

$FrameworkDir = $([System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory())
Set-Alias aspnet_regsql (Join-Path $FrameworkDir "aspnet_regsql.exe")

$artifactPath = "$tmpPackageFolder\content"


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

function Install-Nuget {
    param (
            $PackageName,
			$PackageVersion,
            $SitePath,
            $NugetFeed
        )
		
    #Install the nuget package into the $SitePath
	nuget install $PackageName -Version $PackageVersion -Prerelease -OutputDirectory $SitePath -Source $NugetFeed -NoCache 
}

function Deploy-Nuget {
    param (
            $PackagePath,
            $SitePath,
			$SiteName,
			$DbServer, 
			$DbUsername, 
			$DbPassword,
            $TmpFolder
        )
		
    [System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem')
	
	$tmpPackagePath = "$TmpFolder\" + [guid]::NewGuid().ToString()
    "Extracting $PackagePath to $tmpPackagePath"
    [System.IO.Compression.ZipFile]::ExtractToDirectory($PackagePath, $tmpPackagePath)
    
	if (Test-Path "$tmpPackagePath\content") {
		"Copying package content..."
		Copy-Item "$tmpPackagePath\content\*" $SitePath -recurse -ErrorAction SilentlyContinue
	}
	
	if (Test-Path "$tmpPackagePath\lib") {
		"Copying package assemblies..."
		Copy-Item "$tmpPackagePath\lib\*.dll" "$SitePath\bin"	
	}
	
	if (Test-Path "$tmpPackagePath\tools\epiupdates\sql") {
		$sqlScript = "$tmpPackagePath\Script.sql"				
		foreach ($script in Get-ChildItem "$tmpPackagePath\tools\epiupdates\sql" -Filter '*.sql')
		{
			Add-Content -Path $sqlScript -Value (Get-Content $script.FullName)			
		}
		
		"Executing package SQL scripts..."
		Execute-Sql $sqlScript $SiteName $DbServer $DbUsername $DbPassword
	}	
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

function Execute-Sql {
    param (
        $SqlScript,
        $SiteName,
        $DbServer, 
        $DbUsername, 
        $DbPassword
    )

    "Create Testing database structures: $SqlScript"
	
	$DbName = "daily-$SiteName"
    
	$var1 = "DBName=$DbName"
    $varCollection = $var1
    Invoke-Sqlcmd -InputFile $SqlScript -Variable $varCollection -ServerInstance $DbServer -Username $DbUserName -Password $DbPassword
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
    $DbName = "daily-" + $SiteName
    
	"Transform config files $SitePath\web.config"
	
    Update-EPiXmlFile -TargetFilePath "$SitePath\Web.config" -ModificationFilePath "$tmpPackageFolder\ConnectionString.xmlupdate" -Replaces "{ConnectionStringName}=EPiServerDB;{SqlDataSource}=$DbServer;{DatabaseName}=$DbName;{DatabaseUser}=$DbSiteUser;{DatabasePassword}=$DbSitePassword;"
}



if ($DeleteSite -eq $true) {
    Detach-Database $SiteName $DbServer $DbSiteUser
	Delete-Site $SiteName $SitePath
}

if ($CreateSite -eq $true) {
    Create-Site  $SiteName $SitePath
	
	Install-Nuget $PackageName $PackageVersion $tmpFolder $NugetFeed
	
	Deploy-Zip  $artifactPath\$SiteZip $SitePath
    
	$DbPath = $SitePath + "\App_Data\AlloyEPiServerDB.mdf"    
	Attach-Database $DbPath $SiteName $DbServer $DbSiteUser $DbSitePassword	
	
	Transform-Config $artifactPath $SitePath $DbServer $SiteName $DbSiteUser $DbSitePassword
	
	foreach ($package in Get-ChildItem "$artifactPath" -Filter '*.nupkg') {
		Deploy-Nuget $package.FullName $SitePath $SiteName $DbServer $DbSiteUser $DbSitePassword	$tmpFolder
	}
	
    Copy-Item $LicenseFile $SitePath

	if(Test-Path($tmpFolder)) {
		"Remove temp folder $tmpFolder"
		Remove-Item $tmpFolder -Recurse
	}
}