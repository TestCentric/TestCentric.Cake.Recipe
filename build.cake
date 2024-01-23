// We use some recipe files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/*.cake

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	context: Context,
	title: "TestCentric Cake Recipe",
	githubRepository: "TestCentric.Cake.Recipe");

BuildSettings.Packages.Add(new RecipePackage
(
	id: "TestCentric.Cake.Recipe",
    description: "Cake Recipe used for building TestCentric applications and extensions",
	//releaseNotes: new [] {"line1", "line2", "line3"},
    tags: new [] { "testcentric", "cake", "recipe" }
));

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(CommandLineOptions.Target.Value);
