// All tasks incorporated in the recipe are defined using CakeTaskBuilders.
// The actual specification of criteria, dependencies and actions for each
// task is done separately in task-definitions.cake.
//
// This approach provides a level of indirection, permitting the user to
// modify or completely redefine what a task does in their build.cake file,
// without changing the definitions in the recipe.

public static class BuildTasks
{
	// Our context - set by setup.cake
	public static ICakeContext Context { get; set; }

	// General Tasks
	public static CakeTaskBuilder CheckScriptTask { get; set; }
	public static CakeTaskBuilder DumpSettingsTask { get; set; }
	public static CakeTaskBuilder DefaultTask {get; set; }

	// Help
	public static CakeTaskBuilder HelpTask { get; set; }

	// BuildTasks
	public static CakeTaskBuilder BuildTask { get; set; }
	public static CakeTaskBuilder CheckHeadersTask { get; set; }
	public static CakeTaskBuilder CleanTask { get; set; }
	public static CakeTaskBuilder CleanAllTask { get; set; }
	public static CakeTaskBuilder CleanOutputDirectoriesTask { get; set; }
	public static CakeTaskBuilder CleanAllOutputDirectoriesTask { get; set; }
	public static CakeTaskBuilder CleanPackageDirectoryTask { get; set; }
	public static CakeTaskBuilder DeleteObjectDirectoriesTask { get; set; }
	public static CakeTaskBuilder RestoreTask { get; set; }

	// Unit Testing Task
	public static CakeTaskBuilder UnitTestTask { get; set; }

	// Packaging Tasks
	public static CakeTaskBuilder PackageTask { get; set; }
	public static CakeTaskBuilder BuildPackagesTask { get; set; }
	public static CakeTaskBuilder InstallPackagesTask { get; set; }
	public static CakeTaskBuilder VerifyPackagesTask { get; set; }
	public static CakeTaskBuilder TestPackagesTask { get; set; }

	// Publishing Tasks
	public static CakeTaskBuilder PublishTask { get; set; }
	public static CakeTaskBuilder PublishToLocalFeedTask { get; set; }
	public static CakeTaskBuilder PublishToMyGetTask { get; set; }
	public static CakeTaskBuilder PublishToNuGetTask { get; set; }
	public static CakeTaskBuilder PublishToChocolateyTask { get; set; }

	// Releasing Tasks
	public static CakeTaskBuilder CreateDraftReleaseTask { get; set; }
	public static CakeTaskBuilder DownloadDraftReleaseTask { get; set; }
	public static CakeTaskBuilder UpdateReleaseNotesTask { get; set; } 
	public static CakeTaskBuilder CreateProductionReleaseTask { get; set; }

	// Continuous Integration Task
	public static CakeTaskBuilder ContinuousIntegrationTask { get; set; }
}
