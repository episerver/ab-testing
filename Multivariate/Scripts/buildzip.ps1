$source = $args[0] #"D:\testzip\EPiServer.ConnectForMarketingAutomation.Salesforce"

$destination = $args[1] #"D:\testzip1\EPiServer.ConnectForMarketingAutomation.Salesforce.zip"

 If(Test-path $destination) {Remove-item $destination}

# Add-Type -assembly "system.io.compression.filesystem"
Add-Type -Path "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.IO.Compression.FileSystem.dll"
[io.compression.zipfile]::CreateFromDirectory($Source, $destination)