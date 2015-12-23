$binPath = $env:TF_BUILD_BINARIESDIRECTORY
if (-not($binPath -eq $null))
{
    Copy-Item "$binPath\*.nupkg" "\\amh031\Builds\feed\"
}