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
				"agent-factory.cake", "banner.cake", "build-settings.cake", "builder.cake",
				"chocolatey-package.cake", "command-line-options.cake", "console-reporter.cake",
				"constants.cake", "dotnet-tool-package.cake", "gui-runner.cake", "headers.cake",
				"help-messages.cake", "known-extensions.cake", "nuget-package.cake", "package-checks.cake",
				"package-content.cake", "package-definition.cake", "package-reference.cake",
				"package-test.cake", "publishing.cake", "recipe-package.cake", "recipe.cake",
				"setup.cake", "task-builders.cake", "task-definitions.cake", "test-reports.cake",
				"test-results.cake", "test-runners.cake", "tools.cake", "unit-testing.cake", "versioning.cake" )
        }
	) );

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
