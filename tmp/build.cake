
//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools&version=2019.1.3"
#tool "nuget:?package=xunit.runner.console"

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

// Base component versions

var kpiBaseVersion = "2.5.3";
var kpiCommerceBaseVersion = "2.4.2";
var messagingBaseVersion = "1.3.0";
var webBaseVersion = "2.6.0";

var versionModifiers = new Dictionary<string, string>
{
	{ "release", "pre" },
	{ "dev", "dev" },
	{ "bugfix", "bugfix" },
	{ "feature", "feature" }
};

// Table of project versions

var versions = new Dictionary<string, string>
{
	{ "EPiServer.Marketing.KPI", kpiBaseVersion },
	{ "EPiServer.Marketing.KPI.Commerce", kpiCommerceBaseVersion },
	{ "EPIServer.Marketing.Messaging", messagingBaseVersion },
	{ "EPiServer.Marketing.Testing.Web", webBaseVersion }
};


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

public string AssemblyVersionFor(string projectId)
{
	return versions[projectId];
}

public string FileVersionFor(string projectId)
{
	return $"{versions[projectId]}.{BuildNumber}";
}

public string InformationalVersionFor(string projectId)
{
	string informationalVersion = AssemblyVersionFor(projectId);

	if(isPrereleaseBuild)	
	{
		// Discover the release modifier given the branch type
		var versionModifier = versionModifiers.ContainsKey(branchType)
			? versionModifiers[branchType]
			: versionModifiers["feature"];

		informationalVersion += $"-{versionModifier}-{BuildNumber.ToString("D6")}";
	}

	return informationalVersion;
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
// Task: Version
// Creates an AssemblyInfo.cs, with the appropriate version information,
// for each project with a defined version.
//
Task("Version").IsDependentOn("Describe")
			   .Does(
	() =>
	{	
		foreach(var projectId in versions.Keys)
		{
			var assemblyInfoPath = new FilePath($"../src/{projectId}/AssemblyVersionAuto.cs");
			
			if (FileExists(assemblyInfoPath))
			{
			    DeleteFile(assemblyInfoPath);
			}
			
			CreateAssemblyInfo(
				assemblyInfoPath,
				new AssemblyInfoSettings
				{
					Version = AssemblyVersionFor(projectId),
					InformationalVersion = InformationalVersionFor(projectId),
					Copyright = $"Copyright (c) Episerver {DateTime.Now.Year}"
				}
			);
		}		
	}
);

//
// Task: Build
// Performs a full build of the solution for the specified configuration.
//
Task("Build").IsDependentOn("Describe")
			 .IsDependentOn("Clean")
			 .IsDependentOn("Restore")
			 .IsDependentOn("Version")
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

//
// Task: Test
// Runs all unit tests with DotNetCoreTool and reports their code coverage using DotCoverCover.
//
Task("Test")
	.IsDependentOn("Build")
	.Does(
	() =>
	{
	    foreach(var project in GetFiles("../test/**/*Test.csproj"))
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();
              
			var coverageSettings = new DotCoverCoverSettings
			{
				TargetWorkingDir = project.GetDirectory().FullPath
			}
			.WithFilter("-:*.Test*")					// Exclude Test assemblies
			.WithFilter("-:*MSBuild*")				// Exclude MSBuild assemblies
			.WithAttributeFilter("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");	// Exclude explicitly marked blocks				
			
			DotCoverCover(
				cake => 
				{
					cake.DotNetCoreTool(projectPath: project.FullPath, command: "test", arguments: $"--configuration {configuration} --no-build --no-restore -v m");
				},
				new FilePath($"CodeCoverage/{projectName}.dcvr"),
				coverageSettings
			);     
        } 

        //Merge Code Coverage
        DotCoverMerge(GetFiles("CodeCoverage/*.dcvr"), new FilePath("CodeCoverage/coverage.dcvr"));

		// Report code coverage
		if(isTeamCity)
		{
			var dotCoverToolFilePath = Context.Tools.Resolve("dotCover.exe");
			var dotCoverToolDirectory = dotCoverToolFilePath.GetDirectory();
			var coverageData = MakeAbsolute(new FilePath("./CodeCoverage/coverage.dcvr"));

			TeamCity.ImportDotCoverCoverage(coverageData, dotCoverToolDirectory);
		}
		else 
		{			
			DotCoverReport(
				new FilePath("CodeCoverage/coverage.dcvr"),
				new FilePath("CodeCoverage/coverage.html"),
				new DotCoverReportSettings { ReportType = DotCoverReportType.HTML }
			);
		} 
	}
);

//
// Task: PackageKpi
// Creates a NuGet package for the Kpi project
//
Task("PackageKpi")
	.Does(
    () => {
        var packageVersion = InformationalVersionFor("EPiServer.Marketing.KPI");

        var nuGetPackSettings = new NuGetPackSettings {           
            Version = packageVersion,
            Copyright = $"Copyright Episerver (c) {DateTime.Now.Year}",                                    
            Symbols = false,
            NoPackageAnalysis = true,
            BasePath = $"../src/",
            OutputDirectory = "../Artifacts",
            Files = new []
			{
				new NuSpecContent { Source = $"EPiServer.Marketing.KPI/bin/{configuration}/net461/EPiServer.Marketing.KPI.dll", Target = "lib/net461" },
                new NuSpecContent { Source = $"EPiServer.Marketing.KPI/bin/{configuration}/net461/EPiServer.Marketing.KPI.xml", Target = "lib/net461" },
                new NuSpecContent { Source = $"Database/KPI/1.0.0.1.sql", Target = "tools/epiupdates/sql" }
	        },
            Dependencies = new []
            {
				new NuSpecDependency {}
            }
        };

        NuGetPack($"../src/EPiServer.Marketing.KPI/Package.nuspec", nuGetPackSettings);
	}
);

//
// Task: PackageKpiCommerce
// Creates a NuGet package for the Kpi Commerce project
//
Task("PackageKpiCommerce")
	.Does(
    () => {
        var packageVersion = InformationalVersionFor("EPiServer.Marketing.KPI.Commerce");
		
		CreateDirectory("./module/Admin");
		CopyFileToDirectory("../src/EPiServer.Marketing.KPI.Commerce/Config/CommerceKpiConfig.aspx", "./module/Admin");
		CopyFileToDirectory("../src/EPiServer.Marketing.KPI.Commerce/module.config", "./module");
        Zip("./module", "../EPiServer.Marketing.KPI.Commerce.zip");

        var nuGetPackSettings = new NuGetPackSettings {           
            Version = packageVersion,
            Copyright = $"Copyright Episerver (c) {DateTime.Now.Year}",                                    
            Symbols = false,
            NoPackageAnalysis = true,
            BasePath = $"../src/",
            OutputDirectory = "../Artifacts",
            Files = new []
			{
				new NuSpecContent { Source = $"EPiServer.Marketing.KPI.Commerce/bin/{configuration}/net461/EPiServer.Marketing.KPI.Commerce.dll", Target = "lib/net461" },
                new NuSpecContent { Source = $"EPiServer.Marketing.KPI.Commerce/bin/{configuration}/net461/EPiServer.Marketing.KPI.Commerce.xml", Target = "lib/net461" },
				new NuSpecContent { Source = "../EPiServer.Marketing.KPI.Commerce.zip", Target = "content/modules/_protected/EPiServer.Marketing.KPI.Commerce/" },
				new NuSpecContent { Source = "EPiServer.Marketing.KPI.Commerce/web.config.transform", Target = "content/" }
	        },
            Dependencies = new []
            {
				new NuSpecDependency {Id = "EPiServer.Marketing.KPI", TargetFramework = "net461", Version = "[2.5.3, 3)" },
				new NuSpecDependency {Id = "EPiServer.Commerce.Core", TargetFramework = "net461", Version = "[13.0.0, 14)" }
            }
        };

        NuGetPack($"../src/EPiServer.Marketing.KPI.Commerce/Package.nuspec", nuGetPackSettings);
		
		DeleteDirectory("./module", recursive:true);
		DeleteFile("../EPiServer.Marketing.KPI.Commerce.zip");
	}
);

//
// Task: PackageNuGets
// Roll-up of creation for all Reporting NuGet packages.
//
Task("PackageNuGets")
	.IsDependentOn("PackageKpi")
	.IsDependentOn("PackageKpiCommerce");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Describe")	
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
	.IsDependentOn("PackageNuGets");
    //.IsDependentOn("CollectNuGets")

RunTarget(target);