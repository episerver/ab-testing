#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var branch = Argument("branch", "").ToLower();

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isTeamCity = TeamCity.IsRunningOnTeamCity;

var branchType = branch.Split('/')[0];
var isMasterBranch = branch == "master";
var isPrereleaseBuild = !isMasterBranch;
var isPublicBuild = isMasterBranch || branchType == "release";


//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

public int BuildNumber 
{
	get
	{
		int buildNumber;
		if(!int.TryParse(EnvironmentVariable("BUILD_NUMBER"), out buildNumber))
		{
			buildNumber = 0;
		}

		return buildNumber;
	}
}


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

//
// Task: Prepare
// Prepares the build by determining the environment, branch, 
// component versions, and other factors.
//
Task("Describe").Does(
    () =>
    {
		Information("Build Configuration: {0}", configuration);
		Information("TeamCity Environment: {0}", isTeamCity);
		Information("Branch: {0}", branch);
		Information("Build Number: {0}", BuildNumber);
		Information("Is Public Build: {0}", isPublicBuild);
		Information("Is Production Build: {0}", isMasterBranch);
    }
);

Task("Clean")
    .Does(() =>
{ 
	var path =  $"../src/EPiServer.Marketing.*/**/bin/{configuration}/net461";
	
    CleanDirectories(path);
	CleanDirectories("../Artifacts");
});

//
// Task: Restore
// Restores all NuGet packages for the solution.
//
Task("Restore").Does(
	() => 
	{
		Information($"Using MSBuildSdksPath: {EnvironmentVariable("MSBuildSdksPath")}");

		NuGetRestore(
			"../EPiServer.Marketing.Testing.sln",
			new NuGetRestoreSettings 
			{ 
				ToolPath = "tools/nuget.exe",
				ConfigFile = "../nuget.config",
				NoCache = true,
				Verbosity = NuGetVerbosity.Detailed
			}
		);
	}
);

//
// Task: Build
// Performs a full build of the solution for the specified configuration.
//
Task("Build")
			 .IsDependentOn("Clean")
			 .IsDependentOn("Restore")
			 .IsDependentOn("Describe")
			 //.IsDependentOn("Version")
			 .Does(
	() =>
	{
		Information($"Using MSBuildSdksPath: {EnvironmentVariable("MSBuildSdksPath")}");

        var buildSettings = new MSBuildSettings()
			.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Minimal);
        
        if(isTeamCity)
        {
			// Our old version of TeamCity requires the MSBuild 14 version of the logger
			// even though we build with MSBuild 15.

            buildSettings.WithLogger(
                MakeAbsolute(new DirectoryPath($"../Tools/TeamCity.MSBuild.Logger/msbuild14/TeamCity.MSBuild.Logger.dll")).FullPath, 
                "TeamCity.MSBuild.Logger.TeamCityMSBuildLogger", 
                "teamcity"
            ).SetNoConsoleLogger(true);
        }
        
		MSBuild("../EPiServer.Marketing.Testing.sln",  buildSettings);
	}
);

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Describe")	
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");
    //.IsDependentOn("Test")
	//.IsDependentOn("PackageNuGets")
    //.IsDependentOn("CollectNuGets")
	//.IsDependentOn("PublishToT3");

RunTarget(target);