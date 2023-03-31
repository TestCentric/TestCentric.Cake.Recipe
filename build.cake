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

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	Context,
	"TestCentric.Cake.Recipe");

BuildSettings.Packages.Add( new NuGetPackage
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
			"testcentric-gui.cake",
			"versioning.cake",
			"building.cake",
			"testing.cake",
			"packaging.cake",
			"publishing.cake",
			"releasing.cake")
	}));

	Information($"{BuildSettings.Title} {BuildSettings.Configuration} version {BuildSettings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(BuildSettings.PackageVersion);

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PackageRecipe")
	.Does(() =>
	{
		CreateDirectory(BuildSettings.PackageDirectory);

		NuGetPack("nuget/TestCentric.Cake.Recipe.nuspec", new NuGetPackSettings()
		{
			Version = BuildSettings.PackageVersion,
			OutputDirectory = BuildSettings.PackageDirectory,
			NoPackageAnalysis = true
		});
	});

Task("TestRecipe")
	.IsDependentOn("PackageRecipe");

//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PublishRecipe")
	.IsDependentOn("PackageRecipe")
	.Does(() =>
	{
		if (!BuildSettings.ShouldPublishToMyGet)
			Information("Nothing to publish. Not on main branch.");
		else
		{
			var recipePackage = $"{ BuildSettings.PackageDirectory}{BuildSettings.Title}.{BuildSettings.PackageVersion}.nupkg";
			NuGetPush(recipePackage, new NuGetPushSettings()
			{
				ApiKey = BuildSettings.MyGetApiKey,
				Source = BuildSettings.MyGetPushUrl
			});
		}
	});

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
