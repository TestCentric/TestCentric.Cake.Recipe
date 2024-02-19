public static class BuildTasks
{
	// Help
	public static CakeTaskBuilder HelpTask { get; set; }
    public static CakeTaskBuilder OptionsTask {get; set; }
	public static CakeTaskBuilder TaskListTask { get; set; }

	// General
	public static CakeTaskBuilder CheckScriptTask { get; set; }
	public static CakeTaskBuilder DumpSettingsTask { get; set; }
	public static CakeTaskBuilder DefaultTask {get; set; }

	// Building
	public static CakeTaskBuilder BuildTask { get; set; }
	public static  CakeTaskBuilder CheckHeadersTask { get; set; }
	public static  CakeTaskBuilder CleanTask { get; set; }
	public static  CakeTaskBuilder CleanAllTask { get; set; }
	public static  CakeTaskBuilder CleanOutputDirectoriesTask { get; set; }
	public static  CakeTaskBuilder CleanAllOutputDirectoriesTask { get; set; }
	public static  CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public static  CakeTaskBuilder DeleteObjectDirectoriesTask { get; set; }
	public static  CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing
	public static  CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging
	public static  CakeTaskBuilder PackageTask { get; set; }
	public static  CakeTaskBuilder BuildTestAndPackageTask { get; set; }
	public static  CakeTaskBuilder BuildPackagesTask { get; set; }
	public static  CakeTaskBuilder InstallPackagesTask { get; set; }
	public static  CakeTaskBuilder VerifyPackagesTask { get; set; }
	public static  CakeTaskBuilder TestPackagesTask { get; set; }
	public static  CakeTaskBuilder AddPackagesToLocalFeedTask { get; set; }

	// Publishing
	public static  CakeTaskBuilder PublishTask { get; set; }
	public static  CakeTaskBuilder PublishToMyGetTask { get; set; }
	public static  CakeTaskBuilder PublishToNuGetTask { get; set; }
	public static  CakeTaskBuilder PublishToChocolateyTask { get; set; }

	// Releasing
	public static  CakeTaskBuilder CreateDraftReleaseTask { get; set; }
	public static  CakeTaskBuilder DownloadDraftReleaseTask { get; set; }
	public static  CakeTaskBuilder CreateProductionReleaseTask { get; set; }

    public static string TaskList => $"""
        BUILDING
          {Show(BuildTask)}
          {Show(CleanTask)}
          {Show(CleanAllTask)}
          {Show(CleanOutputDirectoriesTask)}
          {Show(CleanAllOutputDirectoriesTask)}
          {Show(CleanPackageDirectoryTask)}
          {Show(DeleteObjectDirectoriesTask)}
          {Show(RestoreTask)}
          {Show(CheckHeadersTask)}

        UNIT TESTING
          {Show(UnitTestTask)}

        PACKAGING
          {Show(PackageTask)}
          {Show(BuildTestAndPackageTask)}
          {Show(BuildPackagesTask)}
          {Show(AddPackagesToLocalFeedTask)}
          {Show(InstallPackagesTask)}
          {Show(VerifyPackagesTask)}
          {Show(TestPackagesTask)}

        PUBLISHING
          {Show(PublishTask)}
          {Show(PublishToMyGetTask)}
          {Show(PublishToNuGetTask)}
          {Show(PublishToChocolateyTask)}

        RELEASING
          {Show(CreateDraftReleaseTask)}
          {Show(DownloadDraftReleaseTask)}
          {Show(CreateProductionReleaseTask)}

        MISCELLANEOUS
          {Show(CheckScriptTask)}

        MORE INFORMATION
          {Show(HelpTask)}
          {Show(OptionsTask)}
          {Show(TaskListTask)}

        To see all task dependencies use build --tree.
        """;

    private const string THIRTY_SPACES = "                               ";
    private const string DEPENDENCY_PREFIX_1 = "\r\n                                 Depends on: ";
    private const string DEPENDENCY_PREFIX_2 = "\r\n                                             ";
    
    private static string Show(CakeTaskBuilder ctb)
    {
        var sb = new StringBuilder(ctb.Task.Name);
        sb.Append(new string(' ', 29 - sb.Length));
        sb.Append(ctb.Task.Description);

        if (ctb.Task.Dependencies.Count > 0)
        {
            var prefix = DEPENDENCY_PREFIX_1;
            foreach (var dependency in ctb.Task.Dependencies)
            {
                sb.Append(prefix);
                sb.Append(dependency.Name);
                prefix = DEPENDENCY_PREFIX_2;
            }
        }
        
        sb.Append("\r\n");

        return sb.ToString();
    }
}

// The following inline statements do most of the task initialization.
// The tasks created act exactly as if they had been defined in the
// build.cake file of the applcation using the recipe.
//
// The initialization code is inline, so it runs before any tasks but 
// after static initialization. A few tasks need a bit more initialization
// in the BuildSettings constructor as indicated in comments below.

//////////////////////////////////////////////////////////////////////
// HELP TASKS
//////////////////////////////////////////////////////////////////////
BuildTasks.HelpTask = Task("Help")
	.Description("Display help, including Usage, Options and Tasks")
	.Does(() =>
	{
		Information(HelpMessages.Summary);
	});

BuildTasks.OptionsTask = Task("Options")
    .Description("Display command-line options")
    .Does(() =>
    {
        Information(HelpMessages.Options);
    });


BuildTasks.TaskListTask = Task("TaskList")
    .Description("Display tasks provided by the recipe")
    .Does(() =>
    {
        Information(BuildTasks.TaskList);
    });

//////////////////////////////////////////////////////////////////////
// GENERAL TASKS
//////////////////////////////////////////////////////////////////////

BuildTasks.CheckScriptTask = Task("CheckScript")
	.Description("Just make sure the script compiled")
	.Does(() => Information("Script was successfully compiled!"));

BuildTasks.DumpSettingsTask = Task("DumpSettings")
	.Description("Display BuildSettings properties")
	.Does(() => BuildSettings.DumpSettings());

BuildTasks.DefaultTask = Task("Default")
	.Description("Default target if not specified by user")
	.IsDependentOn("Build");

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

BuildTasks.BuildPackagesTask = Task("BuildPackages")
	.Description("Build all packages")
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

BuildTasks.AddPackagesToLocalFeedTask = Task("AddPackagesToLocalFeed")
	.Description("Add packages to our local feed")
	.Does(() =>	{
		if (!BuildSettings.ShouldAddToLocalFeed)
			Information("Nothing to add to local feed from this run.");
		else
			foreach(var package in BuildSettings.Packages)
				if (package.PackageType == PackageType.NuGet || package.PackageType == PackageType.Chocolatey)
					package.AddPackageToLocalFeed();
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
	.Description("Build, Install and verify all packages")
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

		if (calledDirectly)
		{
			if (CommandLineOptions.PackageVersion == null)
				throw new InvalidOperationException("CreateDraftRelease target requires --packageVersion");

			PackageReleaseManager.CreateDraftRelease(CommandLineOptions.PackageVersion.Value);
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

BuildTasks.DownloadDraftReleaseTask = Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>	PackageReleaseManager.DownloadDraftRelease() );

BuildTasks.CreateProductionReleaseTask = Task("CreateProductionRelease")
	.Description("Create a production GitHub Release")
	.Does(() => PackageReleaseManager.CreateProductionRelease() );
