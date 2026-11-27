// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

// Cake.Sdk build for the recipe. Run with: dotnet run --file build.cs -- --target=Package
// or through the build.cmd, build.ps1 and build.sh wrappers.

#:sdk Cake.Sdk@7.0.0
#:package Cake.Git@5.0.1
#:package Cake.MinVer@4.0.0
#:property IncludeAdditionalFiles=recipe/*.cs
#:property Nullable=disable

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
