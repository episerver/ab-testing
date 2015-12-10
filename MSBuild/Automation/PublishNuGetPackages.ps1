Param(
    [Parameter(Mandatory=$true)]
    [string]$packageVersion,
    [Parameter(Mandatory=$true)]
    [string]$destinationPath,
    [Parameter(Mandatory=$true)]
    [string]$branchName)


if (!$destinationPath) {
	return;
}

switch -wildcard ($branchName) {
    "master"   { $preReleaseInfo = "" }
    "master-*" { $preReleaseInfo = "" }
    "release*" { $preReleaseInfo = "-pre-{0:D6}"}
    default    { return; } #only copy to output if this is release or prerelease
}

$packageFolders = @(
	"ABTesting",
	"Multivariate"
)

foreach($package in $packageFolders	) {
	Copy-Item -Path ".\$package\Deployment\*.nupkg" -Destination "$destinationPath"
}