param ()

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd


$SitePath = Resolve-Path "$cwd\..\samples\EPiServer.Templates.Alloy"

&sqllocaldb stop v11.0

# Remove database
Remove-Item "$SitePath\App_Data\*" -include *.mdf,*.ldf -Force

# Remove client resources
$dtkBinaries = "$cwd\..\test\DtkBinaries"
if (Test-Path $dtkBinaries) {
	Remove-Item $dtkBinaries -Force -Recurse
}

# Remove node modules
$nodeModules = "$cwd\..\node_modules"
if (Test-Path $nodeModules) {
	Remove-Item $nodeModules -Force -Recurse
}

# Remove NuGet packages
$packages = "$cwd\..\packages"
if (Test-Path $packages) {
	Remove-Item $packages -Force -Recurse
}

# Remove artifacts
$artifacts = "$cwd\..\artifacts"
if (Test-Path $artifacts) {
	Remove-Item $artifacts -Force -Recurse
}

# Git clean
&git clean -d -x -f ..\

&sqllocaldb start v11.0