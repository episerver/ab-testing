param ()

# Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd


$SitePath = Resolve-Path "$cwd\..\samples\EPiServer.Templates.Alloy"

&sqllocaldb stop v11.0

# Remove database
Remove-Item "$databasePath\*" -include *.mdf,*.ldf

# Git clean
&git clean -d -x -f

&sqllocaldb start v11.0