#tool nuget:?package=GitVersion.CommandLine&version=5.0.0

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string RECIPE_DIR = "recipe/";
const string DEFAULT_VERSION = "1.0.0";
// Dogfooding: We use the recipe to build the recipe package
#load recipe/settings.cake

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

Task("Build")
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

Task("Test")
	.IsDependentOn("Build")
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

Task("Publish")
	.IsDependentOn("Build")
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

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Publish");

Task("Full")
	.IsDependentOn("Build")
	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
