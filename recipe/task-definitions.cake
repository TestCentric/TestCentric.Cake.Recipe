// This file defines what each of the tasks in the recipe actually does.
// You should not change these definitions unless you intend to change
// the behavior of a task for all projects that use the recipe.
//
// To make a change for a single project, you should add code to your build.cake
// or another project-specific cake file. See extending.cake for examples.

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
	.Does(() => Headers.Check());

BuildTasks.CleanTask = Task("Clean")
	.Description("Clean output directories for current config as well as the package directory.")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.IsDependentOn("CleanOutputDirectories")
	.IsDependentOn("CleanPackageDirectory");

BuildTasks.CleanOutputDirectoriesTask = Task("CleanOutputDirectories")
	.Description("Clean output directories for current config.")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() => 
	{
		foreach (var binDir in GetDirectories($"**/bin/{BuildSettings.Configuration}/"))
			CleanDirectory(binDir);
	});

BuildTasks.CleanAllOutputDirectoriesTask = Task("CleanAllOutputDirectories")
	.Description("Clean output directories for all configs.")
	.Does(() =>
	{
		foreach (var binDir in GetDirectories("**/bin/"))
			CleanDirectory(binDir);
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
	.IsDependentOn("CleanAllOutputDirectories")
	.IsDependentOn("CleanPackageDirectory")
	.IsDependentOn("DeleteObjectDirectories");

BuildTasks.DeleteObjectDirectoriesTask = Task("DeleteObjectDirectories")
	.Description("Delete all object directories.")
	.Does(() =>
	{
		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});

BuildTasks.RestoreTask = Task("Restore")
	.Description("Restore all packages referenced by the solution.")
	.WithCriteria(() => BuildSettings.SolutionFile != null)
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() =>
	{
		NuGetRestore(BuildSettings.SolutionFile, BuildSettings.RestoreSettings);
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
		MSBuild(BuildSettings.SolutionFile, BuildSettings.MSBuildSettings.WithProperty("Version", BuildSettings.PackageVersion));
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
	        Banner.Display($"Building {package.PackageFileName}");
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
	        Banner.Display($"Installing {package.PackageFileName}");
			package.InstallPackage();
		}
	});

BuildTasks.VerifyPackagesTask = Task("VerifyPackages")
	.Description("Builds, Installs and Verifies packages. Useful for debugging package content.")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Verifying {package.PackageFileName}");
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
				Banner.Display($"Testing {package.PackageFileName}");
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
    .Does(() =>	{
		if (!BuildSettings.ShouldAddToLocalFeed)
			Information("Nothing to add to local feed from this run.");
		else
			foreach(var package in BuildSettings.Packages)
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
