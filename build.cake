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
		nugetId: "TestCentric.Cake.Recipe",
		guiVersion: "2.0.0-dev00081");

	Information($"{settings.NuGetId} {settings.Configuration} version {settings.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(settings.PackageVersion);

	return settings;
});

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PackageRecipe")
	.Does<BuildSettings>((settings) =>
	{
		CreateDirectory(settings.PackageDirectory);

		NuGetPack("nuget/TestCentric.Cake.Recipe.nuspec", new NuGetPackSettings()
		{
			Version = settings.PackageVersion,
			OutputDirectory = settings.PackageDirectory,
			NoPackageAnalysis = true
		});
	});

Task("TestRecipe")
	.IsDependentOn("PackageRecipe")
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
// PUBLISH PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PublishRecipe")
	.IsDependentOn("PackageRecipe")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.ShouldPublishToMyGet)
			Information("Nothing to publish. Not on main branch.");
		else
			NuGetPush(settings.NuGetPackage, new NuGetPushSettings()
			{
				ApiKey = settings.MyGetApiKey,
				Source = settings.MyGetPushUrl
			});
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// NOTE: We use non-standard task names because the recipe definitions
// of Package, Test and Publish, apply to the normal Clean/Build/Test
// sequence used when creating binary packages.

Task("Appveyor")
	.IsDependentOn("PackageRecipe")
	.IsDependentOn("TestRecipe")
	.IsDependentOn("PublishRecipe");

Task("Full")
	.IsDependentOn("PackageRecipe")
	.IsDependentOn("TestRecipe");

Task("Default")
    .IsDependentOn("PackageRecipe");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
