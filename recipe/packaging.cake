//////////////////////////////////////////////////////////////////////
// PACKAGING TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("BuildPackages")
	.IsDependentOn("VerifyPackages")
	.IsDependentOn("TestPackages");

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGES
//////////////////////////////////////////////////////////////////////

Task("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
			package.BuildPackage();
	});

//////////////////////////////////////////////////////////////////////
// INSTALL PACKAGES
//////////////////////////////////////////////////////////////////////

Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			var packageName = package.PackageName;
			var testDirectory = package.PackageTestDirectory;

			if (System.IO.Directory.Exists(testDirectory))
				DeleteDirectory(testDirectory,
					new DeleteDirectorySettings()
					{
						Recursive = true
					});

			CreateDirectory(testDirectory);

			Unzip(settings.PackageDirectory + packageName, testDirectory);

			Information($"  Installed {packageName}");
			Information($"    at {testDirectory}");
		}
	});

//////////////////////////////////////////////////////////////////////
// CHECK PACKAGE CONTENT
//////////////////////////////////////////////////////////////////////

Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			Information($"Verifying package {package.PackageName}");
			Check.That(package.PackageTestDirectory, package.PackageChecks);
			Information("  SUCCESS: All checks were successful");
		}
	});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGES
//////////////////////////////////////////////////////////////////////

Task("TestPackages")
	.IsDependentOn("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		new GuiRunner(settings, "TestCentric.GuiRunner").InstallRunner();

		foreach (var package in settings.Packages)
		{
			package.TestPackage();
		}
	});
