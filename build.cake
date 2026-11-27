// We use some recipe files for testing. In addition, loading the
// entire recipe gives us an error if any references are missing.
#load recipe/*.cs

//////////////////////////////////////////////////////////////////////
// INITIALIZE BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	context: Context,
	title: "TestCentric Cake Recipe",
	githubRepository: "TestCentric.Cake.Recipe",
	headerCheckDirectories: new[] { "recipe/" },
	defaultTarget: "Package");

//////////////////////////////////////////////////////////////////////
// DEFINE RECIPE PACKAGE
//////////////////////////////////////////////////////////////////////

BuildSettings.Packages.Add(
	new RecipePackage
	(
		id: "TestCentric.Cake.Recipe",
		source: "TestCentric.Cake.Recipe.nuspec",
		checks: new PackageCheck[]
		{
			HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
			HasDirectory("content").WithFiles(
				"Banner.cs", "BuildSettings.cs", "BuildTasks.cs", "BuildVersion.cs", "Builder.cs",
				"ChocolateyPackage.cs", "CommandLineOptions.cs", "ConsoleReporter.cs", "Constants.cs",
				"DotNetToolPackage.cs", "ExtensionSpecifier.cs", "GuiRunner.cs", "Headers.cs",
				"HelpMessages.cs", "NuGetPackage.cs", "PackageChecks.cs", "PackageContent.cs",
				"PackageDefinition.cs", "PackageReference.cs", "PackageReleaseManager.cs", "PackageTest.cs",
				"PackageTestReport.cs", "Recipe.cs", "RecipePackage.cs", "Setup.cs",
				"TestResults.cs", "TestRunners.cs", "Tools.cs", "UnitTesting.cs" )
        }
	) );

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
