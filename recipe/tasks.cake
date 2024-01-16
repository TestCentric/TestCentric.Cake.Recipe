public class BuildTasks
{
	// General
	public CakeTaskBuilder CompileScriptTask { get; set; }
	public CakeTaskBuilder DumpSettingsTask { get; set; }

	// Building
	public CakeTaskBuilder BuildTask { get; set; }
	public CakeTaskBuilder CheckHeadersTask { get; set; }
	public CakeTaskBuilder CleanTask { get; set; }
	public CakeTaskBuilder CleanAllTask { get; set; }
	public CakeTaskBuilder CleanOutputDirectoriesTask { get; set; }
	public CakeTaskBuilder CleanAllOutputDirectoriesTask { get; set; }
	public CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public CakeTaskBuilder DeleteObjectDirectoriesTask { get; set; }
	public CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing
	public CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging
	public CakeTaskBuilder PackageTask { get; set; }
	public CakeTaskBuilder BuildTestAndPackageTask { get; set; }
	public CakeTaskBuilder BuildPackagesTask { get; set; }
	public CakeTaskBuilder InstallPackagesTask { get; set; }
	public CakeTaskBuilder VerifyPackagesTask { get; set; }
	public CakeTaskBuilder TestPackagesTask { get; set; }
	public CakeTaskBuilder AddPackagesToLocalFeedTask { get; set; }

	// Publishing
	public CakeTaskBuilder PublishTask { get; set; }
	public CakeTaskBuilder PublishToMyGetTask { get; set; }
	public CakeTaskBuilder PublishToNuGetTask { get; set; }
	public CakeTaskBuilder PublishToChocolateyTask { get; set; }

	// Releasing
	public CakeTaskBuilder CreateDraftReleaseTask { get; set; }
	public CakeTaskBuilder DownloadDraftReleaseTask { get; set; }
	public CakeTaskBuilder CreateProductionReleaseTask { get; set; }
}

// The following inline statements do most of the task initialization.
// They run before any tasks but after static initialization. A few
// tasks need a bit more initialization in the BuildSettings constructor
// as indicated in comments below.

//////////////////////////////////////////////////////////////////////
// GENERAL TASKS
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.CompileScriptTask = Task("CompileScript")
	.Description("Just make sure the script compiled")
	.Does(() => Information("Script was successfully compiled!"));

BuildSettings.Tasks.DumpSettingsTask = Task("DumpSettings")
	.Does(() => BuildSettings.DumpSettings());

//////////////////////////////////////////////////////////////////////
// BUILDING TASKS
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.CheckHeadersTask = Task("CheckHeaders")
	.Description("Check source files for valid copyright headers")
	.WithCriteria(() => !BuildSettings.SuppressHeaderCheck)
	.Does(() => Headers.Check());

BuildSettings.Tasks.CleanTask = Task("Clean")
	.Description("Clean output and package directories")
	.IsDependentOn("CleanOutputDirectories")
	.IsDependentOn("CleanPackageDirectory");

BuildSettings.Tasks.CleanOutputDirectoriesTask = Task("CleanOutputDirectories")
	.Description("Clean output directories for current config")
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() => 
	{
		foreach (var binDir in GetDirectories($"**/bin/{BuildSettings.Configuration}/"))
			CleanDirectory(binDir);
	});

BuildSettings.Tasks.CleanAllOutputDirectoriesTask = Task("CleanAllOutputDirectories")
	.Description("Clean output directories for all configs")
	.Does(() =>
	{
		foreach (var binDir in GetDirectories("**/bin/"))
			CleanDirectory(binDir);
	});

BuildSettings.Tasks.CleanPackageDirectoryTask = Task("CleanPackageDirectory")
	.Description("Clean the package directory")
	.Does(() => CleanDirectory(BuildSettings.PackagingDirectory));

BuildSettings.Tasks.CleanAllTask = Task("CleanAll")
	.Description("Clean all output directories, package directory and delete all obj directories")
	.IsDependentOn("CleanAllOutputDirectories")
	.IsDependentOn("CleanPackageDirectory")
	.IsDependentOn("DeleteObjectDirectories");

BuildSettings.Tasks.DeleteObjectDirectoriesTask = Task("DeleteObjectDirectories")
	.Description("Delete all obj directories")
	.Does(() =>
	{
		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});

BuildSettings.Tasks.RestoreTask = Task("Restore")
	.Description("Restore referenced packages")
	.WithCriteria(() => BuildSettings.SolutionFile != null)
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.Does(() =>
	{
		NuGetRestore(BuildSettings.SolutionFile, BuildSettings.RestoreSettings);
	});


BuildSettings.Tasks.BuildTask = Task("Build")
	.WithCriteria(() => BuildSettings.SolutionFile != null)
	.WithCriteria(() => !CommandLineOptions.NoBuild)
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.IsDependentOn("CheckHeaders")
	.Description("Build The solution")
	.Does(() =>
	{
		MSBuild(BuildSettings.SolutionFile, BuildSettings.MSBuildSettings.WithProperty("Version", BuildSettings.PackageVersion));
	});

//////////////////////////////////////////////////////////////////////
// UNIT TEST TASK
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.UnitTestTask = Task("Test")
	.Description("Run unit tests")
	.IsDependentOn("Build")
	.Does(() => UnitTestRunner.RunUnitTests());

//////////////////////////////////////////////////////////////////////
// PACKAGING TASKS
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.PackageTask = Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

BuildSettings.Tasks.BuildTestAndPackageTask = Task("BuildTestAndPackage")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

BuildSettings.Tasks.BuildPackagesTask = Task("BuildPackages")
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

BuildSettings.Tasks.AddPackagesToLocalFeedTask = Task("AddPackagesToLocalFeed")
	.Description("Add packages to our local feed")
	.Does(() =>	{
		if (!BuildSettings.ShouldAddToLocalFeed)
			Information("Nothing to add to local feed from this run.");
		else
			foreach(var package in BuildSettings.Packages)
				if (package.PackageType == PackageType.NuGet || package.PackageType == PackageType.Chocolatey)
					package.AddPackageToLocalFeed();
	});

BuildSettings.Tasks.InstallPackagesTask = Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Installing {package.PackageFileName}");
			package.InstallPackage();
		}
	});

BuildSettings.Tasks.VerifyPackagesTask = Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
		{
	        Banner.Display($"Verifying {package.PackageFileName}");
			package.VerifyPackage();
		}
	});

BuildSettings.Tasks.TestPackagesTask = Task("TestPackages")
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

BuildSettings.Tasks.PublishTask = Task("Publish")
	.Description("Publish nuget and chocolatey packages according to the current settings")
	.IsDependentOn("Package")
	.Does(() => PackageReleaseManager.Publish());

// This task may be run independently when recovering from errors.
BuildSettings.Tasks.PublishToMyGetTask = Task("PublishToMyGet")
	.Description("Publish packages to MyGet")
	.Does(() =>	PackageReleaseManager.PublishToMyGet());

// This task may be run independently when recovering from errors.
BuildSettings.Tasks.PublishToNuGetTask = Task("PublishToNuGet")
	.Description("Publish packages to NuGet")
	.Does(() =>	PackageReleaseManager.PublishToNuGet());

// This task may be run independently when recovering from errors.
BuildSettings.Tasks.PublishToChocolateyTask = Task("PublishToChocolatey")
	.Description("Publish packages to Chocolatey")
	.Does(() =>	PackageReleaseManager.PublishToChocolatey());

BuildSettings.Tasks.CreateDraftReleaseTask = Task("CreateDraftRelease")
	.Does(() =>
	{
		bool calledDirectly = CommandLineOptions.Target == "CreateDraftRelease";

		if (calledDirectly)
		{
			if (CommandLineOptions.PackageVersion == null)
				throw new InvalidOperationException("CreateDraftRelease target requires --packageVersion");

			PackageReleaseManager.CreateDraftRelease(CommandLineOptions.PackageVersion);
		}
		else if (!BuildSettings.IsReleaseBranch)
		{
			Information("Skipping creation of draft release because this is not a release branch");
		}
		else
		{
			PackageReleaseManager.CreateDraftRelease(BuildSettings.BuildVersion.BranchName.Substring(8));
		}
	});

BuildSettings.Tasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildSettings.Tasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
	.Does(() => PackageReleaseManager.CreateProductionRelease() );
