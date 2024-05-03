// We use some recipe files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/*.cake

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	context: Context,
	title: "TestCentric Cake Recipe",
	githubRepository: "TestCentric.Cake.Recipe",
	defaultTarget: "Package" );

//////////////////////////////////////////////////////////////////////
// DEFINE RECIPE PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new RecipePackage
	(
		id: "TestCentric.Cake.Recipe",
		description: "Cake Recipe used for building TestCentric applications and extensions",
		//releaseNotes: new [] {"line1", "line2", "line3"},
		tags: new [] { "testcentric", "cake", "recipe" }
	) );

Task("TestLatest")
	.Does(() =>
	{
		TestLatest("TestCentric.Extension.Net462PluggableAgent");
		TestLatest("TestCentric.Engine");
		TestLatest("testcentric-extension-net462-pluggable-agent");
		TestLatest("testcentric-gui");
		TestLatest("TestCentric.GuiRunner");
	});

private void TestLatest(string id)
{
	var package = new PackageReference(id, "1.2.3");
	var latestRelease = package.LatestRelease.Version;
	var latestDevBuild = package.LatestDevBuild.Version;
	Console.WriteLine($"{id}  {latestRelease}  {latestDevBuild}");
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
