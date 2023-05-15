public class BuildTasks
{
	// General
	public CakeTaskBuilder DumpSettingsTask { get; set; }

	// Building
	public CakeTaskBuilder BuildTask { get; set; }
	public CakeTaskBuilder CheckHeadersTask { get; set; }
	public CakeTaskBuilder CleanTask { get; set; }
	public CakeTaskBuilder CleanAllTask { get; set; }
	public CakeTaskBuilder CleanOutputDirectoryTask { get; set; }
	public CakeTaskBuilder CleanAllOutputDirectoriesTask { get; set; }
	public CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public CakeTaskBuilder DeleteObjectDirectoriesTask { get; set; }
	public CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing
	public CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging
	public CakeTaskBuilder PackageTask { get; set; }
	public CakeTaskBuilder PackageExistingBuildTask { get; set; }
	public CakeTaskBuilder BuildTestAndPackageTask { get; set; }
	public CakeTaskBuilder BuildPackagesTask { get; set; }
	public CakeTaskBuilder InstallPackagesTask { get; set; }
	public CakeTaskBuilder VerifyPackagesTask { get; set; }
	public CakeTaskBuilder TestPackagesTask { get; set; }

	// Publishing
	public CakeTaskBuilder PublishTask { get; set; }
	public CakeTaskBuilder PublishToMyGetTask { get; set; }
	public CakeTaskBuilder PublishToNuGetTask { get; set; }
	public CakeTaskBuilder PublishToChocolateyTask { get; set; }

	// Releasing
	public CakeTaskBuilder CreateDraftReleaseTask { get; set; }
	public CakeTaskBuilder DownloadDraftReleaseTask { get; set; }
	public CakeTaskBuilder CreateProductionReleaseTask { get; set; }

	// While most dependencies are fixed, some of them vary according
	// to the build settings. This method is called after the settings
	// have been initialized.
	public void FixupDependencies()
	{
		// Dependencies that change when there is no solution file.
		if (BuildSettings.SolutionFile != null)
		{
			BuildTask.IsDependentOn("Clean").IsDependentOn("Restore").IsDependentOn("CheckHeaders");
			PackageTask.IsDependentOn("Build");
		}
		else
		{
			PackageTask.IsDependentOn("CleanPackageDirectory");
		}
	}
}

// The following inline statements do most of the task initialization.
// They run before any tasks but after static initialization. The
// build settings have not yet been fully initialized at this point,
// which is why we need FixupDependencies.

//////////////////////////////////////////////////////////////////////
// DUMPSETTINGS TASK
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.DumpSettingsTask = Task("DumpSettings")
	.Does(() => BuildSettings.DumpSettings());

//////////////////////////////////////////////////////////////////////
// BUILDING TASKS
//////////////////////////////////////////////////////////////////////

BuildSettings.Tasks.CheckHeadersTask = Task("CheckHeaders")
	.Description("Check source files for valid copyright headers")
	.Does(() => Headers.Check());

BuildSettings.Tasks.CleanTask = Task("Clean")
	.Description("Clean output and package directories")
	.IsDependentOn("CleanOutputDirectory")
	.IsDependentOn("CleanPackageDirectory");

BuildSettings.Tasks.CleanOutputDirectoryTask = Task("CleanOutputDirectory")
	.Description("Clean output directory for current config")
	.Does(() => CleanDirectory(BuildSettings.OutputDirectory));

BuildSettings.Tasks.CleanAllOutputDirectoriesTask = Task("CleanAllOutputDirectories")
	.Description("Clean output directories for all configs")
	.Does(() => CleanDirectory(BuildSettings.ProjectDirectory + "bin/"));

BuildSettings.Tasks.CleanPackageDirectoryTask = Task("CleanPackageDirectory")
	.Description("Clean the package directory")
	.Does(() => CleanDirectory(BuildSettings.PackageDirectory));

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
	.Does(() =>
	{
		if (BuildSettings.SolutionFile == null)
			throw new Exception("Nothing to restore because there is no solution file");

		NuGetRestore(BuildSettings.SolutionFile, BuildSettings.RestoreSettings);
	});


BuildSettings.Tasks.BuildTask = Task("Build")
	.Description("Build The solution")
	.Does(() =>
	{
		if (BuildSettings.SolutionFile == null)
			throw new Exception("Nothing to build because there is no solution file");

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
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

BuildSettings.Tasks.PackageExistingBuildTask = Task("PackageExistingBuild")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

BuildSettings.Tasks.BuildTestAndPackageTask = Task("BuildTestAndPackage")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

BuildSettings.Tasks.BuildPackagesTask = Task("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildPackage();
	});

BuildSettings.Tasks.InstallPackagesTask = Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.InstallPackage();
	});

BuildSettings.Tasks.VerifyPackagesTask = Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.VerifyPackage();
	});

BuildSettings.Tasks.TestPackagesTask = Task("TestPackages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.RunPackageTests();
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
	.Does(() => PackageReleaseManager.CreateDraftRelease() );

BuildSettings.Tasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildSettings.Tasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
	.Does(() => PackageReleaseManager.CreateProductionRelease() );
