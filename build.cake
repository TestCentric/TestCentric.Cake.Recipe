#tool nuget:?package=GitVersion.CommandLine&version=5.0.0

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string NUGET_ID = "TestCentric.Cake.Recipe";
const string CHOCO_ID = "NONE";
const string RECIPE_DIR = "recipe/";
const string DEFAULT_VERSION = "1.0.0";
// Dogfooding: We use the recipe to build the recipe package
#load recipe/parameters.cake

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////

Setup<BuildParameters>((context) =>
{
	var parameters = new BuildParameters(context);
	parameters.GuiVersion = "2.0.0-dev00081";

	Information($"{NUGET_ID} {parameters.Configuration} version {parameters.PackageVersion}");

	if (BuildSystem.IsRunningOnAppVeyor)
		AppVeyor.UpdateBuildVersion(parameters.PackageVersion);

	return parameters;
});

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGE
//////////////////////////////////////////////////////////////////////

Task("BuildPackage")
	.Does<BuildParameters>((parameters) =>
	{
		CreateDirectory(parameters.PackageDirectory);

		NuGetPack("nuget/TestCentric.Cake.Recipe.nuspec", new NuGetPackSettings()
		{
			Version = parameters.PackageVersion,
			OutputDirectory = parameters.PackageDirectory,
			NoPackageAnalysis = true
		});
	});

Task("TestPackage")
	.IsDependentOn("BuildPackage")
	.IsDependentOn("TestGuiInstall");

Task("TestGuiInstall")
	.Does<BuildParameters>((parameters) =>
	{
		Information($"Installing {GuiRunner.NuGetId}.{parameters.GuiVersion}...\n");

		CreateDirectory(parameters.PackageTestDirectory);
		CleanDirectory(parameters.PackageTestDirectory);

		new GuiRunner(parameters, GuiRunner.NuGetId).InstallRunner();

		Information("Verifying the installation...");

		Check.That(parameters.PackageTestDirectory,
			HasDirectory($"{GuiRunner.NuGetId}.{parameters.GuiVersion}")
				.WithFiles("LICENSE.txt", "NOTICES.txt", "CHANGES.txt"));

		Information("\nGUI was successfully installed!");
	});

//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGE
//////////////////////////////////////////////////////////////////////

Task("PublishPackage")
	.IsDependentOn("BuildPackage")
	.Does<BuildParameters>((parameters) =>
	{
		if (!parameters.ShouldPublishToMyGet)
			Information("Nothing to publish. Not on main branch.");
		else
			NuGetPush(parameters.NuGetPackage, new NuGetPushSettings()
			{
				ApiKey = EnvironmentVariable(MYGET_API_KEY),
				Source = MYGET_PUSH_URL
			});
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("BuildPackage")
	.IsDependentOn("TestPackage")
	.IsDependentOn("PublishPackage");

Task("Full")
	.IsDependentOn("BuildPackage")
	.IsDependentOn("TestPackage");

Task("Default")
    .IsDependentOn("BuildPackage");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
