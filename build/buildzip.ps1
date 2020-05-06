$source = $args[0]
$destination = $args[1]

#Make sure the script runs in the right context, might be wrong if started from e.g. .cmd file
$cwd = Split-Path -parent $PSCommandPath
pushd $cwd

If(Test-path $destination) 
{
	Remove-item $destination
}

# Add-Type -assembly "system.io.compression.filesystem"
Add-Type -Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.IO.Compression.FileSystem.dll"

[io.compression.zipfile]::CreateFromDirectory($Source, $destination)