#tool nuget:?package=GitVersion.CommandLine&version=5.6.3

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string RECIPE_DIR = "recipe/";

// We use the some files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/building.cake
#load recipe/build-settings.cake
#load recipe/check-headers.cake
#load recipe/constants.cake
#load recipe/package-checks.cake
#load recipe/package-definition.cake
#load recipe/package-tests.cake
#load recipe/packaging.cake
#load recipe/publishing.cake
#load recipe/releasing.cake
#load recipe/setup.cake
#load recipe/testing.cake
#load recipe/test-reports.cake
#load recipe/test-results.cake
#load recipe/test-runners.cake
#load recipe/utilities.cake
#load recipe/versioning.cake

var target = Argument("target", Argument("t", "Default"));

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	Context,
	"TestCentric.Cake.Recipe");

var recipePackage = new NuGetPackage
(
	id: "TestCentric.Cake.Recipe",
	source: "nuget/TestCentric.Cake.Recipe.nuspec",
	basePath: "nuget",
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt", "testcentric.png"),
		HasDirectory("content").WithFiles(
			"check-headers.cake",
			"package-checks.cake",
			"package-definition.cake",
			"test-results.cake",
			"test-reports.cake",
			"package-tests.cake",
			"test-runners.cake",
			"versioning.cake",
			"building.cake",
			"testing.cake",
			"packaging.cake",
			"publishing.cake",
			"releasing.cake")
	});

	BuildSettings.Packages.Add(recipePackage);

	Information($"{BuildSettings.Title} {BuildSettings.Configuration} version {BuildSettings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(BuildSettings.PackageVersion);

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PackageRecipe")
	.Does(() =>
	{
		recipePackage.BuildVerifyAndTest();
	});

//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PublishRecipe")
	.IsDependentOn("PackageRecipe")
	.IsDependentOn("PublishToMyGet")
	.IsDependentOn("PublishToNuGet");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// NOTE: We use non-standard task names because the recipe definitions
// of Package, Test and Publish, apply to the normal Clean/Build/Test
// sequence used when creating binary packages.

Task("Appveyor")
	.IsDependentOn("PackageRecipe")
	.IsDependentOn("PublishRecipe");

Task("Default")
    .IsDependentOn("PackageRecipe");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
