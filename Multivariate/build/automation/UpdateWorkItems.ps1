Param(
    [Parameter(Mandatory=$true)]
    [string]$packageVersion,
    [Parameter(Mandatory=$true)]
    [string]$branch)

Function BranchExists {
    Param([string]$branchName)

    $branchesUri = "https://stash.ep.se/rest/api/1.0/projects/NAS/repos/episerver.connectforsharepoint/branches?filterText=$branchName"
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f "buildenv", "3p1server")))

    try {
        $matches = (Invoke-RestMethod $branchesUri -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} `
                       | Select-Object -ExpandProperty values | Select-Object -ExpandProperty displayId)
        return $matches -contains $branchName
    } catch {
        throw "Error listing branches: $($_.Exception.Message)"
    }
}

Function GetMajorVersion {
    Param([string]$packageVersion)

    if ($packageVersion -match "\d+") {
        return $matches[0]
    } else {
        throw "Failed to find major version number in package version."
    }
}

Function SendUpdate {
	Param(
	    [string]$packageVersion,
	    [string]$branch)

    $defaultPackage = "EPiServer.Commerce"
    if($branch -match "master*")
    {
        $body = @{
            StashProject = "NAS"
            StashRepo = "episerver.connectforsharepoint"
            Version = "$packageVersion"
            DefaultPackage = $defaultPackage
			AreaPaths = @("ConnectForSharepoint")
        } | ConvertTo-Json
        $uri = "http://tfs01vm:8080/TfsStash/api/ReleasedInByTag"
        Invoke-RestMethod -Method Post -Uri $uri -ContentType "application/json" -Body $body
    }

    if($branch -match "release*")
    {
        # If there is a master branch for the major version, e.g. "master-7", diff from that, otherwise use "master"
        $majorVersion = GetMajorVersion $packageVersion
        $masterBranch = "master-$majorVersion"

        if (!(BranchExists $masterBranch)) {
            $masterBranch = "master"
        }

        $body = @{
            StashProject = "NAS"
            StashRepo = "episerver.connectforsharepoint"
            Version = "$packageVersion"
            Since = $masterBranch
            Until = $branch
            DefaultPackage = $defaultPackage
			AreaPaths = @("ConnectForSharepoint")
        } | ConvertTo-Json
        $uri = "http://tfs01vm:8080/TfsStash/api/ReleasedIn"
        Invoke-RestMethod -Method Post -Uri $uri -ContentType "application/json" -Body $body
    }
}

Function SendUpdateToJira {
    Param(
        [string]$packageVersion,
        [string]$branch)

    $defaultPackage = "EPiServer.ConnectForSharepoint"
    if($branch -match "master*")
    {
        $body = @{
            StashProject = "NAS"
            StashRepo = "episerver.connectforsharepoint"
            JiraProject= "MAI"
            AutoResolveBugs = $true
            Version = "$packageVersion"
            DefaultPackage = $defaultPackage
        }
        $uri = "http://tfs01vm:8080/TfsStash/api/JiraReleasedInByTag"
        Invoke-RestMethod -Method Post -Uri $uri -Body $body
    }

    if($branch -match "release*")
    {
        # If there is a master branch for the major version, e.g. "master-7", diff from that, otherwise use "master"
        $majorVersion = GetMajorVersion $packageVersion
        $masterBranch = "master-$majorVersion"

        if (!(BranchExists $masterBranch)) {
            $masterBranch = "master"
        }

        $body = @{
            StashProject = "NAS"
            StashRepo = "episerver.connectforsharepoint"
            JiraProject= "MAI"
            AutoResolveBugs = $true
            Version = "$packageVersion"
            Since = $masterBranch
            Until = $branch
            DefaultPackage = $defaultPackage
        }
        $uri = "http://tfs01vm:8080/TfsStash/api/JiraReleasedIn"
        Invoke-RestMethod -Method Post -Uri $uri -Body $body
    }
}

SendUpdate $packageVersion $branch
SendUpdateToJira $packageVersion $branch