public static class BuildTasks
{
	// General
	public static CakeTaskBuilder CheckScriptTask { get; set; }
	public static CakeTaskBuilder DumpSettingsTask { get; set; }
	public static CakeTaskBuilder DefaultTask {get; set; }

	// Building
	public static CakeTaskBuilder BuildTask { get; set; }
	public static CakeTaskBuilder CheckHeadersTask { get; set; }
	public static CakeTaskBuilder CleanTask { get; set; }
	public static CakeTaskBuilder CleanAllTask { get; set; }
	public static CakeTaskBuilder CleanOutputDirectoriesTask { get; set; }
	public static CakeTaskBuilder CleanAllOutputDirectoriesTask { get; set; }
	public static CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public static CakeTaskBuilder DeleteObjectDirectoriesTask { get; set; }
	public static CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing
	public static CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging
	public static CakeTaskBuilder PackageTask { get; set; }
	public static CakeTaskBuilder BuildTestAndPackageTask { get; set; }
	public static CakeTaskBuilder PackageBuildTask { get; set; }
	public static CakeTaskBuilder PackageInstallTask { get; set; }
	public static CakeTaskBuilder PackageVerifyTask { get; set; }
	public static CakeTaskBuilder PackageTestTask { get; set; }

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
	public static CakeTaskBuilder AppveyorTask { get; set; }
}
