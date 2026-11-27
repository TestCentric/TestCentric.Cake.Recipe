// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
// All tasks incorporated in the recipe are defined using CakeTaskBuilders.
// The actual specification of criteria, dependencies and actions for each
// task is done separately, later in this file.
//
// This approach provides a level of indirection, permitting the user to
// modify or completely redefine what a task does in their build.cake file,
// without changing the definitions in the recipe.

public static class BuildTasks
{
	// Our context - set by BuildSettings.Initialize
	public static ICakeContext Context { get; set; }

	// General
	public static CakeTaskBuilder CheckScriptTask { get; set; }
	public static CakeTaskBuilder DumpSettingsTask { get; set; }
	public static CakeTaskBuilder DefaultTask {get; set; }

	// Help
	public static CakeTaskBuilder HelpTask { get; set; }

	// Building
	public static CakeTaskBuilder BuildTask { get; set; }
	public static CakeTaskBuilder CheckHeadersTask { get; set; }
	public static CakeTaskBuilder CleanTask { get; set; }
	public static CakeTaskBuilder CleanAllTask { get; set; }
	public static CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public static CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing
	public static CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging
	public static CakeTaskBuilder PackageTask { get; set; }
	public static CakeTaskBuilder BuildPackagesTask { get; set; }
	public static CakeTaskBuilder InstallPackagesTask { get; set; }
	public static CakeTaskBuilder VerifyPackagesTask { get; set; }
	public static CakeTaskBuilder TestPackagesTask { get; set; }

	// Publishing
	public static CakeTaskBuilder PublishTask { get; set; }
	public static CakeTaskBuilder PublishToLocalFeedTask { get; set; }
	public static CakeTaskBuilder PublishToMyGetTask { get; set; }
	public static CakeTaskBuilder PublishToNuGetTask { get; set; }
	public static CakeTaskBuilder PublishToChocolateyTask { get; set; }

	// Releasing
	public static CakeTaskBuilder CreateDraftReleaseTask { get; set; }
	public static CakeTaskBuilder DownloadDraftReleaseTask { get; set; }
	public static CakeTaskBuilder UpdateReleaseNotesTask { get; set; } 
	public static CakeTaskBuilder CreateProductionReleaseTask { get; set; }

	// Continuous Integration
	public static CakeTaskBuilder ContinuousIntegrationTask { get; set; }
}

// The following statements initialize each of the defined tasks. They run in
// DefineTasks(), which BuildSettings.Initialize calls. They define what each of the tasks in the recipe actually does.
// You should not change these definitions unless you intend to change
// the behavior of a task for all projects that use the recipe.
//
// To make a change for a single project, you should add code to your
// build.cake file or another project-specific cake file.

static void DefineTasks()
{
//////////////////////////////////////////////////////////////////////
// GENERAL TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.CheckScriptTask = Task("CheckScript")
	.Description("Verify that the script compiles.")
	.Does(() => Information("Script was successfully compiled!"));

BuildTasks.DumpSettingsTask = Task("DumpSettings")
	.Description("""
		Display build settings so that you can verify that your script has
		initialized them correctly.
		""")
	.Does(() => BuildSettings.DumpSettings());

//////////////////////////////////////////////////////////////////////
// HELP TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.HelpTask = Task("Help")
	.Description("""
		Display help info for the recipe package. The default display shows general
		usage information, including available options. For a list of available
		targets, add the --tasks option.
		""")
	.Does(() => 
	{
		if (CommandLineOptions.ShowTasks)
			Information(HelpMessages.Tasks);
		else
			Information(HelpMessages.Usage);
	});

//////////////////////////////////////////////////////////////////////
// BUILDING TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.CheckHeadersTask = Task("CheckHeaders")
	.Description("""
		Check source files for valid copyright headers. Currently, only C# files
		are checked. Normally a standard TestCentric header is used but a project
		may specify a different header when initializing BuildSettings.
		""")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.WithCriteria(() => !BuildSettings.SuppressHeaderCheck)
	.Does(() => CheckHeaders());

BuildTasks.CleanTask = Task("Clean")
	.Description("Clean output directories for current config as well as the package directory.")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.IsDependentOn("CleanPackageDirectory")
	.Does(() =>
     {
         foreach (var binDir in GetDirectories($"**/bin/{BuildSettings.Configuration}/"))
             CleanDirectory(binDir);

         CleanDirectory(BuildSettings.PackageDirectory);
     });

BuildTasks.CleanPackageDirectoryTask = Task("CleanPackageDirectory")
	.Description("Clean the package directory.")
	// TODO: Test with Package task
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() => CleanDirectory(BuildSettings.PackageDirectory));

BuildTasks.CleanAllTask = Task("CleanAll")
	.Description("""
		Clean all output directories and package directory and
		delete all object directories.
		""")
	.Does(() =>
	{
        foreach (var binDir in GetDirectories("**/bin/"))
            CleanDirectory(binDir);

		CleanDirectory(BuildSettings.PackageDirectory);

        foreach (var dir in GetDirectories("src/**/obj/"))
            DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
    });

BuildTasks.RestoreTask = Task("Restore")
	.Description("Restore all packages referenced by the solution.")
	.WithCriteria(() => BuildSettings.SolutionFile != null)
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() =>
	{
        DotNetRestore(BuildSettings.SolutionFile);
	});


BuildTasks.BuildTask = Task("Build")
	.WithCriteria(() => BuildSettings.SolutionFile != null)
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.IsDependentOn("CheckHeaders")
	.Description("""
        Compiles the code in your solution. If there is no solution in the
        project, the command is not available and an error is displayed if
        you enter it.
        """)
	.Does(() =>
	{
		// TODO: Change back to DotNetBuild once TestCentric-Gui can use it, i.e.
		//   DotNetBuild(BuildSettings.SolutionFile, BuildSettings.DotNetBuildSettings);
		// DotNetBuildSettings sets GenerateResourceUsePreserializedResources, so each
		// .NET Framework project with non-string resources then needs a reference to
		// System.Resources.Extensions, and the GUI packages must ship that DLL and its
		// dependencies (System.Memory, System.Buffers, System.Numerics.Vectors and
		// System.Runtime.CompilerServices.Unsafe). Without them the GUI build fails
		// with MSB3822, or the packaged GUI fails at startup.
		MSBuild(BuildSettings.SolutionFile, BuildSettings.MSBuildSettings
			.WithProperty("Version", BuildSettings.PackageVersion));
	});

//////////////////////////////////////////////////////////////////////
// UNIT TEST TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.UnitTestTask = Task("Test")
	.Description("""
        Does Build and then runs your unit tests if you have any. If you are
        certain that nothing in your code has changed, you can use `--nobuild` to
        eliminate the compilation step.
        """)
	.IsDependentOn("Build")
	.Does(() => UnitTesting.RunAllTests());

//////////////////////////////////////////////////////////////////////
// PACKAGING TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.PackageTask = Task("Package")
	.IsDependentOn("Build")
	.Description("""
        Builds, installs, verifies and tests all the packages you have defined.
        Verification is based on the checks you define for each package. Testing
        uses the tests you have specified for each package. If you are certain
        that nothing in your code has changed, you can use `--nobuild` to
        eliminate the compilation step.
        """)
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

BuildTasks.BuildPackagesTask = Task("BuildPackages")
	.Description("""
		Compiles your application and then builds the packages. Use --nobuild to skip
		compilation. Use for debugging the building of packages.
		""")
	.IsDependentOn("Build")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        DisplayBanner($"Building {package.PackageFileName}");
			package.BuildPackage();

			if (BuildSettings.ShouldAddToLocalFeed)
				if (package.PackageType == PackageType.NuGet || package.PackageType == PackageType.Chocolatey)
					package.AddPackageToLocalFeed();
		}
	});

BuildTasks.InstallPackagesTask = Task("InstallPackages")
	.Description("Builds and installs packages. Useful for debugging installation.")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        DisplayBanner($"Installing {package.PackageFileName}");
			package.InstallPackage();
		}
	});

BuildTasks.VerifyPackagesTask = Task("VerifyPackages")
	.Description("Builds, Installs and Verifies packages. Useful for debugging package content.")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        DisplayBanner($"Verifying {package.PackageFileName}");
			package.VerifyPackage();
		}
	});

BuildTasks.TestPackagesTask = Task("TestPackages")
	.Description("Builds, Installs and runs package tests. Particularly useful in combination\r\nwith the --where option to debug a single package.")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
			if (package.PackageTests != null)
			{
				DisplayBanner($"Testing {package.PackageFileName}");
				package.RunPackageTests();
			}
		}
	});

//////////////////////////////////////////////////////////////////////
// PUBLISHING TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.PublishTask = Task("Publish")
	.Description("""
        Publishes packages to MyGet, NuGet or Chocolatey, based on the
        branch being built and the package version. Although this task
        is not dependent on the PublishToMyget, PublishToNuGet or
        PublishTo Chocolatey tasks, it calls the same underlying logic
        to determine what should be published.
        """)
	.IsDependentOn("Package")
	.Does(() => PackageReleaseManager.Publish());

BuildTasks.PublishToLocalFeedTask = Task("PublishToLocalFeed")
    .Description("""
	Publishes packages to the local feed for a dev, alpha, beta, or rc build
	or for a final release. If not, or if the --nopush option was used,
	a message is displayed.
	""")
	.WithCriteria(() => BuildSettings.IsLocalBuild)
    .Does(() =>	{
		if (!BuildSettings.ShouldAddToLocalFeed)
			Information("Nothing to add to local feed from this run.");
        else if (!SIO.Directory.Exists(BuildSettings.LocalPackagesDirectory))
            throw new Exception("Local packages directory not found");
        else
            foreach (var package in BuildSettings.Packages)
				if (package.PackageType == PackageType.NuGet || package.PackageType == PackageType.Chocolatey)
					package.AddPackageToLocalFeed();
	});

// This task may be run independently when recovering from errors.
BuildTasks.PublishToMyGetTask = Task("PublishToMyGet")
	.Description("""
		Publishes packages to MyGet for a dev build. If not, or if the --nopush
		option was used, a message is displayed.
		""")
	.Does(() =>	PackageReleaseManager.PublishToMyGet());

// This task may be run independently when recovering from errors.
BuildTasks.PublishToNuGetTask = Task("PublishToNuGet")
    .Description("""
	Publishes packages to NuGet for an alpha, beta, rc or final release. If not,
	or if the --nopush option was used, a message is displayed.
	""")
    .Does(() =>	PackageReleaseManager.PublishToNuGet());

// This task may be run independently when recovering from errors.
BuildTasks.PublishToChocolateyTask = Task("PublishToChocolatey")
    .Description("""
	Publishes packages to Chocolatey for an alpha, beta, rc or final release.
	If not, or if the --nopush option was used, a message is displayed.
	""")
	.Does(() =>	PackageReleaseManager.PublishToChocolatey());

BuildTasks.CreateDraftReleaseTask = Task("CreateDraftRelease")
	.Description("""
        Creates a draft release for a milestone on GitHub. The milestone name must
        match the three-part package version for each package. This target will fail
        with an error message if no milestone is found or if it doesn't meet criteria
        for a draft release.
        """)
	.Does(() => PackageReleaseManager.CreateDraftRelease() );

BuildTasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("""
		Download draft release for local use
		""")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildTasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
    .Description("""
        Creates a production release for a milestoneon GitHub. The milestone name
        must match the three-part package version for each package. This target will
        fail with an error message if no milestone is found or if it doesn't meet
        criteria for a production release.
        """)
    .Does(() => PackageReleaseManager.CreateProductionRelease() );

BuildTasks.UpdateReleaseNotesTask = Task("UpdateReleaseNotes")
	.Description("Create a production GitHub Release")
	.Does(() => PackageReleaseManager.UpdateReleaseNotes() );

//////////////////////////////////////////////////////////////////////
// CONTINUOUS INTEGRATION TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.ContinuousIntegrationTask = Task("ContinuousIntegration")
	.Description("""
		Perform a continuous integration run, using dependent tasks to build and 
		unit test the software, create, install, verify and test packages. If run
		on a release branch (release-x.y.z), it will also create a draft release.
		If run on main, it will publish the packages and create a full production
		release on GitHub assuming no failures occur.

		This task will normally only be run on a CI server. For a given release, 
		it should only be run on the CI server selected to perform releases. Other 
		targets must be selected for any additional serviers in use.
		""")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

//////////////////////////////////////////////////////////////////////
// DEFAULT TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.DefaultTask = Task("Default")
	.Description("""
		Default target if none is specified on the command-line. This is normally set
		to Build but may be changed when calling BuildSettings.Initialize().
		""");
}
}
