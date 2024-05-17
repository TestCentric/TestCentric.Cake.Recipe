// The following inline statements do most of the task initialization.
// The tasks created act exactly as if they had been defined in the
// build.cake file of the applcation using the recipe.
//
// The initialization code is inline, so it runs before any tasks but 
// after static initialization. A few tasks need a bit more initialization
// in the BuildSettings constructor as indicated in comments below.

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
	.Does(() => CleanDirectory(BuildSettings.PackagingDirectory));

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

BuildTasks.BuildTestAndPackageTask = Task("BuildTestAndPackage")
	.Description("Do Build, Test and Package all in one run")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

BuildTasks.PackageBuildTask = Task("PackageBuild")
	.Description("Build any packages without re-compiling")
	.IsDependentOn("CleanPackageDirectory")
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

BuildTasks.PackageInstallTask = Task("PackageInstall")
	.Description("Build and Install any packages without re-compiling")
	.IsDependentOn("PackageBuild")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Installing {package.PackageFileName}");
			package.InstallPackage();
		}
	});

BuildTasks.PackageVerifyTask = Task("PackageVerify")
	.Description("Build, Install and verify any packages without re-compiling")
	.IsDependentOn("PackageInstall")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Verifying {package.PackageFileName}");
			package.VerifyPackage();
		}
	});

BuildTasks.PackageTestTask = Task("PackageTest")
	.Description("Build, Install and Test any packages without re-compiling")
	.IsDependentOn("PackageInstall")
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
	.Description("Add any Nuget or Chocolatey packages we have built to our local feed")
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
	.Does(() =>
	{
		bool calledDirectly = CommandLineOptions.Target.Value == "CreateDraftRelease";

		if (CommandLineOptions.PackageVersion.Exists)
			PackageReleaseManager.CreateDraftRelease(CommandLineOptions.PackageVersion.Value);
		else if (BuildSettings.IsReleaseBranch)
			PackageReleaseManager.CreateDraftRelease(BuildSettings.BuildVersion.BranchName.Substring(8));
		else if (calledDirectly)
			throw new InvalidOperationException("CreateDraftRelease target requires --packageVersion");
		else
			Information("Skipping creation of draft release because this is not a release branch");
	});

BuildTasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildTasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
	.Description("Create a production GitHub Release")
	.Does(() => PackageReleaseManager.CreateProductionRelease() );

//////////////////////////////////////////////////////////////////////
// CONTINUOUS INTEGRATION TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.ContinuousIntegrationTask = Task("ContinuousIntegration")
	.Description("Perform continuous integration run")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	//.IsDependentOn("CreateDraftRelease") Issue 67
	.IsDependentOn("CreateProductionRelease");

BuildTasks.AppveyorTask = Task("Appveyor")
	.Description("Target for running on AppVeyor")
	.IsDependentOn("ContinuousIntegration");

//////////////////////////////////////////////////////////////////////
// DEFAULT TASK
//////////////////////////////////////////////////////////////////////

BuildTasks.DefaultTask = Task("Default")
	.Description("Default target if not specified by user")
	.IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

public Builder Build => CommandLineOptions.Usage
    ? new Builder(() => Information(HelpMessages.Usage))
    : new Builder(() => RunTarget(CommandLineOptions.Target.Value));

public class Builder
{
    private Action _action;

    public Builder(Action action)
    {
        _action = action;
    }

    public void Run()
    {
        _action();
    }
}