Param(
    [Parameter(Mandatory=$true)]
    [string]$packageVersion,
    [Parameter(Mandatory=$true)]
    [string]$destinationPath)

 $packagesToPublish = @(
	"EPiServer.ConnectForSharePoint"
)

Function Copy-NugetPackages {
	Param(
	    [string]$packageVersion,
	    [string]$destinationPath)
	# Dont copy anything if the release build is triggered on something other than master or release branch
	# if (($packageVersion -match "-") -and !($packageVersion -match "-pre-")) {
		# return;
	# }
	foreach($package in $packagesToPublish	) {
	    Copy-Item -Path "$package.$packageVersion.nupkg" -Destination "$destinationPath"
	}

	# Copy these packages only if build is a prerelease version
	# if ($packageVersion -match "-") {
		# foreach($package in $packagesToPublishOnlyInPrerelease	) {
	    	# Copy-Item -Path "$package.$packageVersion.nupkg" -Destination "$destinationPath"
		# }
	# }
}

if (!$destinationPath) {
	return;
}

Copy-NugetPackages $packageVersion $destinationPath