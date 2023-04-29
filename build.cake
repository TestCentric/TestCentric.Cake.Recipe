#tool NuGet.CommandLine&version=6.0.0
#tool nuget:?package=GitVersion.CommandLine&version=5.6.3

// We use some recipe files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	context: Context,
	title: "TestCentric.Cake.Recipe",
	githubRepository: "TestCentric.Cake.Recipe");

var recipePackage = new RecipePackage
(
	id: "TestCentric.Cake.Recipe",
	source: "nuget/TestCentric.Cake.Recipe.nuspec",
	basePath: "nuget",
	content: new FilePath[] {
		"building.cake",
		"BuildSettings.cake",
		"check-headers.cake",
		"ConsoleReporter.cake",
		"constants.cake",
		"package-checks.cake",
		"package-definitions.cake",
		"PackageTest.cake",
		"packaging.cake",
		"publishing.cake",
		"RecipePackage.cake",
		"releasing.cake",
		"setup.cake",
		"testing.cake",
		"test-reports.cake",
		"test-results.cake",
		"test-runners.cake",
		"utilities.cake",
		"versioning.cake"
	});

	BuildSettings.Packages.Add(recipePackage);

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("Package")
	.IsDependentOn("Publish");

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
