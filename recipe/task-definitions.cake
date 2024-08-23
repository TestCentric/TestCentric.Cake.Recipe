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
	.Description("Just make sure the script compiled")
	.Does(() => Information("Script was successfully compiled!"));

BuildTasks.DumpSettingsTask = Task("DumpSettings")
	.Description("Display BuildSettings properties")
	.Does(() => BuildSettings.DumpSettings());

//////////////////////////////////////////////////////////////////////
// HELP TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.HelpTask = Task("Help")
	.Description("Display Help Message")
	.Does(() => 
	{
		// TODO: Add additional message selections
		Information(HelpMessages.Usage);
	});

//////////////////////////////////////////////////////////////////////
// BUILDING TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.CheckHeadersTask = Task("CheckHeaders")
	.Description("Check source files for valid copyright headers")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.WithCriteria(() => !BuildSettings.SuppressHeaderCheck)
	.Does(() => Headers.Check());

BuildTasks.CleanTask = Task("Clean")
	.Description("Clean output and package directories")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.IsDependentOn("CleanOutputDirectories")
	.IsDependentOn("CleanPackageDirectory");

BuildTasks.CleanOutputDirectoriesTask = Task("CleanOutputDirectories")
	.Description("Clean output directories for current config")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() => 
	{
		foreach (var binDir in GetDirectories($"**/bin/{BuildSettings.Configuration}/"))
			CleanDirectory(binDir);
	});

BuildTasks.CleanAllOutputDirectoriesTask = Task("CleanAllOutputDirectories")
	.Description("Clean output directories for all configs")
	.Does(() =>
	{
		foreach (var binDir in GetDirectories("**/bin/"))
			CleanDirectory(binDir);
	});

BuildTasks.CleanPackageDirectoryTask = Task("CleanPackageDirectory")
	.Description("Clean the package directory")
	// TODO: Test with Package task
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() => CleanDirectory(BuildSettings.PackageDirectory));

BuildTasks.CleanAllTask = Task("CleanAll")
	.Description("Clean everything!")
	.IsDependentOn("CleanAllOutputDirectories")
	.IsDependentOn("CleanPackageDirectory")
	.IsDependentOn("DeleteObjectDirectories");

BuildTasks.DeleteObjectDirectoriesTask = Task("DeleteObjectDirectories")
	.Description("Delete all obj directories")
	.Does(() =>
	{
		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});

BuildTasks.RestoreTask = Task("Restore")
	.Description("Restore referenced packages")
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
	.Description("Build the solution")
	.Does(() =>
	{
		MSBuild(BuildSettings.SolutionFile, BuildSettings.MSBuildSettings.WithProperty("Version", BuildSettings.PackageVersion));
	});

//////////////////////////////////////////////////////////////////////
// UNIT TEST TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.UnitTestTask = Task("Test")
	.Description("Run unit tests")
	.IsDependentOn("Build")
	.Does(() => UnitTesting.RunAllTests());

//////////////////////////////////////////////////////////////////////
// PACKAGING TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.PackageTask = Task("Package")
	.IsDependentOn("Build")
	.Description("Build, Install, Verify and Test all packages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

BuildTasks.BuildPackagesTask = Task("BuildPackages")
	.Description("Build all packages")
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
	.Description("Build and Install all packages")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Installing {package.PackageFileName}");
			package.InstallPackage();
		}
	});

BuildTasks.VerifyPackagesTask = Task("VerifyPackages")
	.Description("Build, Install and Verify all packages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Verifying {package.PackageFileName}");
			package.VerifyPackage();
		}
	});

BuildTasks.TestPackagesTask = Task("TestPackages")
	.Description("Build, Install and Test all packages")
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
	.Description("Publish all packages for current branch")
	.IsDependentOn("Package")
	.Does(() => PackageReleaseManager.Publish());

BuildTasks.PublishToLocalFeedTask = Task("PublishToLocalFeed")
	.Description("Add Nuget and Chocolatey packages to local feed")
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
	.Description("Publish packages to MyGet")
	.Does(() =>	PackageReleaseManager.PublishToMyGet());

// This task may be run independently when recovering from errors.
BuildTasks.PublishToNuGetTask = Task("PublishToNuGet")
	.Description("Publish packages to NuGet")
	.Does(() =>	PackageReleaseManager.PublishToNuGet());

// This task may be run independently when recovering from errors.
BuildTasks.PublishToChocolateyTask = Task("PublishToChocolatey")
	.Description("Publish packages to Chocolatey")
	.Does(() =>	PackageReleaseManager.PublishToChocolatey());

BuildTasks.CreateDraftReleaseTask = Task("CreateDraftRelease")
	.Description("Create a draft release on GitHub")
	.Does(() => PackageReleaseManager.CreateDraftRelease() );

BuildTasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildTasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
	.Description("Create a production GitHub Release")
	.Does(() => PackageReleaseManager.CreateProductionRelease() );

BuildTasks.UpdateReleaseNotesTask = Task("UpdateReleaseNotes")
	.Description("Create a production GitHub Release")
	.Does(() => PackageReleaseManager.UpdateReleaseNotes() );

//////////////////////////////////////////////////////////////////////
// CONTINUOUS INTEGRATION TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.ContinuousIntegrationTask = Task("ContinuousIntegration")
	.Description("Perform continuous integration run")
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
	.Description("Default target if not specified by user")
	.IsDependentOn("Build");
