
//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NUnit.ConsoleRunner&version=3.4.0"
#tool "nuget:?package=JetBrains.dotCover.CommandLineTools&version=2019.1.3"
#tool "nuget:?package=xunit.runner.console"
#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"

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
var runAnalysis = isTeamCity && isMasterBranch;

// Base component versions

var kpiBaseVersion = "2.5.5";
var kpiCommerceBaseVersion = "2.4.2";
var messagingBaseVersion = "1.3.0";
var webBaseVersion = "2.6.8";

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

public void SignAssembly(string assemblyFullPath)
{
	var exitCode = StartProcess("sn", new ProcessSettings{ Arguments = $"-q -Rc \"..\\src\\{assemblyFullPath}\" \"EPiServerProduct\"" });
	Information("Exit code: {0}", exitCode);
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
public void StopProcessesByName(string processName)
{
	Information($"Stopping {processName} processes.");
	try
	{
		foreach(var process in System.Diagnostics.Process.GetProcessesByName(processName))
		{
			Information($"Stopping process {process.Id} ({process.MainModule.FileName})");

			try
			{
				process.Kill();
			}
			catch (Exception ex)
			{
				Warning($"Could not stop process {process.Id} ({process.MainModule.FileName}): {ex.Message}");
			}
		}
	}
	catch(Exception ex)
	{
		Warning($"Could not stop {processName} processes: {ex.Message}");
	}
}

//
// Teardown
// Cleans up processes and/or resources lingering after a build.
//
Teardown(
	context =>
	{
		StopProcessesByName("dotnet");
	}
);

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
	CleanDirectories("CodeCoverage");
	CleanDirectories(".sonarqube");
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
// Task: StartAnalysis
// Start SonarQube analysis.
//
Task("StartAnalysis")
	.WithCriteria(runAnalysis)
	.ContinueOnError()
	.Does(
	() =>
	{
		var analysisInclusions = new []
		{
			"**/src/**/*"
		};

		SonarBegin(
			new SonarBeginSettings {
				Key = "MAR",
				Version = InformationalVersionFor("EPiServer.Marketing.Testing.Web"),
				DotCoverReportsPath = MakeAbsolute(new FilePath("CodeCoverage/coverage.html")).FullPath,
				Inclusions = string.Join(",", analysisInclusions),
				Url = EnvironmentVariable("SonarQubeUrl"),
				Login = EnvironmentVariable("SonarQubeLoginKey"),
				UseCoreClr = false
			}
		);
	})
	.ReportError(ex => Warning(ex.ToString()));

//
// Task: Build
// Performs a full build of the solution for the specified configuration.
//
Task("Build").IsDependentOn("Describe")
			 .IsDependentOn("Clean")
			 .IsDependentOn("Restore")
			 .IsDependentOn("Version")
			 .IsDependentOn("StartAnalysis")
			 .Does(
	() =>
	{
		Information($"Using MSBuildSdksPath: {EnvironmentVariable("MSBuildSdksPath")}");

        var buildSettings = new MSBuildSettings()
			.SetConfiguration(configuration)
			.SetVerbosity(Verbosity.Minimal);

		MSBuild("../EPiServer.Marketing.Testing.sln",  buildSettings);
	}
);

//
// Task: SignAssemblies
// Signs the assemblies.
//
Task("SignAssemblies")
	.IsDependentOn("Build")
	.Does(
	() =>
	{
		// Prepare the PATH so that the build routine can find the tools it
		// requires to run successfully.
		var signingToolPath = GetFiles("C:/Program Files (x86)/Microsoft SDKs/Windows/*/bin/*Tools/x64/sn.exe")
								.Select(t => t.GetDirectory().FullPath)
								.FirstOrDefault();

		if(string.IsNullOrWhiteSpace(signingToolPath))
		{
			throw new Exception("No assembly signing tools could be found on this build agent.");
		}

		Environment.SetEnvironmentVariable("Path", $"{signingToolPath};{EnvironmentVariable("Path")}");

		SignAssembly($"EPiServer.Marketing.KPI/bin/{configuration}/net461/EPiServer.Marketing.KPI.dll");
		SignAssembly($"EPiServer.Marketing.KPI.Commerce/bin/{configuration}/net461/EPiServer.Marketing.KPI.Commerce.dll");
		SignAssembly($"EPIServer.Marketing.Messaging/bin/{configuration}/net461/EPIServer.Marketing.Messaging.dll");
		SignAssembly($"EPiServer.Marketing.Testing.Web/bin/{configuration}/net461/EPiServer.Marketing.Testing.Web.dll");
		SignAssembly($"EPiServer.Marketing.Testing.Dal/bin/{configuration}/net461/EPiServer.Marketing.Testing.Dal.dll");
		SignAssembly($"EPiServer.Marketing.Testing.Core/bin/{configuration}/net461/EPiServer.Marketing.Testing.Core.dll");
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

			if(projectName != "EPiServer.Marketing.Testing.Web.ClientTest")
			{
				var coverageSettings = new DotCoverCoverSettings
				{
					TargetWorkingDir = project.GetDirectory().FullPath
				}
				.WithFilter("-:*EPiServer.Marketing.KPI.Test*")
				.WithFilter("-:*EPiServer.Marketing.KPI.Commerce.Test*")
				.WithFilter("-:*EPiServer.Marketing.Messaging.Test*")
				.WithFilter("-:*EPiServer.Marketing.Testing.Test*")
				.WithFilter("-:*xunit.assert*")
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

		DotCoverReport(
			new FilePath("CodeCoverage/coverage.dcvr"),
			new FilePath("CodeCoverage/coverage.html"),
			new DotCoverReportSettings { ReportType = DotCoverReportType.HTML }
		);
	}
);

//
// Task: StopAnalysis
// Complete SonarQube analysis and submit results.
//
Task("StopAnalysis")
	.IsDependentOn("Test")
	.WithCriteria(runAnalysis)
	.ContinueOnError()
	.Does(
	() =>
	{
		SonarEnd(
			new SonarEndSettings {
				Login = EnvironmentVariable("SonarQubeLoginKey"),
				UseCoreClr = false
			}
		);
	})
	.ReportError(ex => Warning(ex.ToString()));

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
				new NuSpecDependency {Id = "EntityFramework", TargetFramework = "net461", Version = "[6.2.0, 7)" },
				new NuSpecDependency {Id = "EPiServer.CMS.UI.Core", TargetFramework = "net461", Version = "[11.2.5, 12)" }
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
		var kpiVersion = InformationalVersionFor("EPiServer.Marketing.KPI");

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
				new NuSpecDependency {Id = "EPiServer.Marketing.KPI", TargetFramework = "net461", Version = $"[{kpiVersion}, 3)" },
				new NuSpecDependency {Id = "EPiServer.Commerce.Core", TargetFramework = "net461", Version = "[13.0.0, 14)" }
            }
        };

        NuGetPack($"../src/EPiServer.Marketing.KPI.Commerce/Package.nuspec", nuGetPackSettings);

		DeleteDirectory("./module", new DeleteDirectorySettings { Recursive = true, Force = true });
		DeleteFile("../EPiServer.Marketing.KPI.Commerce.zip");
	}
);

//
// Task: PackageMessaging
// Creates a NuGet package for the Messaging project
//
Task("PackageMessaging")
	.Does(
    () => {
        var packageVersion = InformationalVersionFor("EPIServer.Marketing.Messaging");

        var nuGetPackSettings = new NuGetPackSettings {
            Version = packageVersion,
            Copyright = $"Copyright Episerver (c) {DateTime.Now.Year}",
            Symbols = false,
            NoPackageAnalysis = true,
            BasePath = $"../src/",
            OutputDirectory = "../Artifacts",
            Files = new []
			{
				new NuSpecContent { Source = $"EPIServer.Marketing.Messaging/bin/{configuration}/net461/EPIServer.Marketing.Messaging.dll", Target = "lib/net461" },
                new NuSpecContent { Source = $"EPIServer.Marketing.Messaging/bin/{configuration}/net461/EPIServer.Marketing.Messaging.xml", Target = "lib/net461" }
	        },
            Dependencies = new []
            {
				new NuSpecDependency {}
            }
        };

        NuGetPack($"../src/EPIServer.Marketing.Messaging/Package.nuspec", nuGetPackSettings);
	}
);

//
// Task: PackageABTesting
// Creates a NuGet package for the AB Testing project
//
Task("PackageABTesting")
	.Does(
    () => {
        var packageVersion = InformationalVersionFor("EPiServer.Marketing.Testing.Web");
		var kpiVersion = InformationalVersionFor("EPiServer.Marketing.KPI");
		var messagingVersion = InformationalVersionFor("EPIServer.Marketing.Messaging");

		CreateDirectory("./module/Admin");
		CopyFileToDirectory("../src/EPiServer.Marketing.Testing.Web/Config/AdminConfig.aspx", "./module/Admin");
		CopyDirectory("../src/EPiServer.Marketing.Testing.Web/ClientResources", "./module/ClientResources");
		CopyDirectory("../src/EPiServer.Marketing.Testing.Web/EmbeddedLangFiles", "./module/EmbeddedLangFiles");
		CopyDirectory("../src/EPiServer.Marketing.Testing.Web/Images", "./module/Images");
		CopyFileToDirectory("../src/EPiServer.Marketing.Testing.Web/module.config", "./module");
        Zip("./module", "../EPiServer.Marketing.Testing.zip");

		var nuspecContentList = new List<NuSpecContent>();
		foreach(var file in GetFiles("../src/Database/Testing/*.sql"))
		{
		   nuspecContentList.Add(new NuSpecContent { Source = "../src/Database/Testing/" + file.GetFilename().ToString(), Target = "tools/epiupdates/sql" });
		}

		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Web/bin/{configuration}/net461/EPiServer.Marketing.Testing.Web.dll", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Web/bin/{configuration}/net461/EPiServer.Marketing.Testing.Web.xml", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Dal/bin/{configuration}/net461/EPiServer.Marketing.Testing.Dal.dll", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Dal/bin/{configuration}/net461/EPiServer.Marketing.Testing.Dal.xml", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Core/bin/{configuration}/net461/EPiServer.Marketing.Testing.Core.dll", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = $"EPiServer.Marketing.Testing.Core/bin/{configuration}/net461/EPiServer.Marketing.Testing.Core.xml", Target = "lib/net461" });
		nuspecContentList.Add(new NuSpecContent { Source = "../EPiServer.Marketing.Testing.zip", Target = "content/modules/_protected/EPiServer.Marketing.Testing/" });
		nuspecContentList.Add(new NuSpecContent { Source = "EPiServer.Marketing.Testing.Web/web.config.transform", Target = "content/" });
		nuspecContentList.Add(new NuSpecContent { Source = "EPiServer.Marketing.Testing.Web/web.config.install.xdt", Target = "content/" });

        var nuGetPackSettings = new NuGetPackSettings {
            Version = packageVersion,
            Copyright = $"Copyright Episerver (c) {DateTime.Now.Year}",
            Symbols = false,
            NoPackageAnalysis = true,
            BasePath = $"../src/",
            OutputDirectory = "../Artifacts",
            Files = nuspecContentList.ToArray(),
            Dependencies = new []
            {
				new NuSpecDependency {Id = "EPiServer.Marketing.KPI", TargetFramework = "net461", Version = $"[{kpiVersion}, 3)" },
				new NuSpecDependency {Id = "EPiServer.Marketing.Messaging", TargetFramework = "net461", Version = $"[{messagingVersion}, 2)" },
				new NuSpecDependency {Id = "EPiServer.CMS.AspNet", TargetFramework = "net461", Version = "[11.3.3, 12)" },
				new NuSpecDependency {Id = "EPiServer.CMS.Core", TargetFramework = "net461", Version = "[11.3.3, 12)" },
				new NuSpecDependency {Id = "EPiServer.CMS.UI", TargetFramework = "net461", Version = "[11.2.5, 12)" },
				new NuSpecDependency {Id = "EPiServer.CMS.UI.Core", TargetFramework = "net461", Version = "[11.2.5, 12)" },
				new NuSpecDependency {Id = "EPiServer.Framework", TargetFramework = "net461", Version = "[11.8.0, 12)" },
				new NuSpecDependency {Id = "EPiServer.Framework.AspNet", TargetFramework = "net461", Version = "[11.3.3, 12)" },
				new NuSpecDependency {Id = "Microsoft.AspNet.WebApi", TargetFramework = "net461", Version = "[5.2.3, 6)" },
				new NuSpecDependency {Id = "Microsoft.AspNet.Mvc", TargetFramework = "net461", Version = "[5.2.3, 6)" }
            }
        };

        NuGetPack($"../src/EPiServer.Marketing.Testing.Web/Package.nuspec", nuGetPackSettings);

		DeleteDirectory("./module", new DeleteDirectorySettings { Recursive = true, Force = true });
		DeleteFile("../EPiServer.Marketing.Testing.zip");
	}
);

//
// Task: PackageNuGets
// Roll-up of creation for all NuGet packages.
//
Task("PackageNuGets")
	.IsDependentOn("PackageKpi")
	.IsDependentOn("PackageKpiCommerce")
	.IsDependentOn("PackageMessaging")
	.IsDependentOn("PackageABTesting");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Describe")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
	.IsDependentOn("StartAnalysis")
    .IsDependentOn("Build")
	.IsDependentOn("SignAssemblies")
    .IsDependentOn("Test")
	.IsDependentOn("StopAnalysis")
	.IsDependentOn("PackageNuGets");

RunTarget(target);