#tool nuget:?package=GitVersion.CommandLine&version=5.0.0

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string RECIPE_DIR = "recipe/";

// We use the some files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/BuildSettings.cake

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////

Setup<BuildSettings>((context) =>
{
	var settings = BuildSettings.Initialize(
		context: context,
		title: "TestCentric.Cake.Recipe",
		guiVersion: "2.0.0-dev00081");

	settings.Packages.Add
	(
		new NuGetPackage
		(
			settings,
			"TestCentric.Cake.Recipe",
			"nuget/TestCentric.Cake.Recipe.nuspec"
		)
		{
			PackageChecks = new PackageCheck[]
			{
				HasFiles("LICENSE.txt", "testcentric.png"),
				HasDirectory("content").WithFiles(
					"HeaderCheck.cake",
					"PackageCheck.cake",
					"PackageDefinition.cake",
					"test-results.cake",
					"test-reports.cake",
					"package-tests.cake",
					"GuiRunner.cake",
					"BuildVersion.cake",
					"building.cake",
					"testing.cake",
					"packaging.cake",
					"publishing.cake",
					"releasing.cake")
			}
		}
	);

	Information($"{settings.Title} {settings.Configuration} version {settings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(settings.PackageVersion);

	return settings;
});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGE
//////////////////////////////////////////////////////////////////////

Task("TestRecipe")
	.IsDependentOn("Package")
	.IsDependentOn("TestGuiInstall");

Task("TestGuiInstall")
	.Does<BuildSettings>((settings) =>
	{
		Information($"Installing {GuiRunner.NuGetId}.{settings.GuiVersion}...\n");

		CreateDirectory(settings.PackageTestDirectory);
		CleanDirectory(settings.PackageTestDirectory);

		new GuiRunner(settings, GuiRunner.NuGetId).InstallRunner();

		Information("Verifying the installation...");

		Check.That(settings.PackageTestDirectory,
			HasDirectory($"{GuiRunner.NuGetId}.{settings.GuiVersion}")
				.WithFiles("LICENSE.txt", "NOTICES.txt", "CHANGES.txt"));

		Information("\nGUI was successfully installed!");
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// NOTE: We use non-standard task names because the recipe definitions
// of Package, Test and Publish, apply to the normal Clean/Build/Test
// sequence used when creating binary packages.

Task("Appveyor")
	.IsDependentOn("Package")
	.IsDependentOn("TestRecipe")
	.IsDependentOn("Publish");

Task("Full")
	.IsDependentOn("Package")
	.IsDependentOn("TestRecipe");

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
