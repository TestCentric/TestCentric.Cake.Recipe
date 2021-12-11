#tool nuget:?package=GitVersion.CommandLine&version=5.0.0

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string RECIPE_DIR = "recipe/";

// We use the some files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/build-settings.cake

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////

Setup<BuildSettings>((context) =>
{
	var settings = BuildSettings.Initialize(
		context: context,
		title: "TestCentric.Cake.Recipe",
		guiVersion: "2.0.0-dev00081",
		packages: new[] { new NuGetPackage
		(
			id: "TestCentric.Cake.Recipe",
			source: "nuget/TestCentric.Cake.Recipe.nuspec",
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
			}
		)}
	);

	Information($"{settings.Title} {settings.Configuration} version {settings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(settings.PackageVersion);

	return settings;
});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGE
//////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// NOTE: We use non-standard task names because the recipe definitions
// of Package, Test and Publish, apply to the normal Clean/Build/Test
// sequence used when creating binary packages.

Task("Appveyor")
	.IsDependentOn("Package")
	.IsDependentOn("Publish");

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
