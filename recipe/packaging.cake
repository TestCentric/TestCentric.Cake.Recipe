//////////////////////////////////////////////////////////////////////
// PACKAGING TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

Task("PackageExistingBuild")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

Task("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildPackage();
	});

Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.InstallPackage();
	});

Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.VerifyPackage();
	});

/*
//////////////////////////////////////////////////////////////////////
// BUILD PACKAGES
//////////////////////////////////////////////////////////////////////

Task("BuildPackages")
	.Does(() =>
	{
		foreach (var package in BuildSettings.Packages)
			if (package.PackageTYpe == PackageTYpe.NuGet)
			{
				CreateDirectory(BuildSettings.PackageDirectory);
				NuGetPack(package.PackageSource, new NuGetPackSettings()
				{
					Version = BuildSettings.PackageVersion,
					OutputDirectory = BuildSettings.PackageDirectory,
					NoPackageAnalysis = true
				});
			}
			else if (package.PackageType == PackageType.Chocolatey)
			{
                CreateDirectory(BuildSettings.PackageDirectory);
                ChocolateyPack(package.PackageSource, new ChocolateyPackSettings()
                {
                    Version = BuildSettings.PackageVersion,
                    OutputDirectory = BuildSettings.PackageDirectory
                });
			}
	});

//////////////////////////////////////////////////////////////////////
// INSTALL PACKAGES
//////////////////////////////////////////////////////////////////////

Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does(() =>
	{
		foreach (var package in BuildSettings.Packages)
		{
			var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
			var testDirectory = BuildSettings.PackageTestDirectory + package.PackageId;

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
	.Does(() =>
	{
		foreach (var package in BuildSettings.Packages)
		{
			var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
			Information($"Verifying package {packageName}");
			var testDirectory = BuildSettings.PackageTestDirectory + package.PackageId;
			Check.That(testDirectory, package.PackageChecks);
			Information("  SUCCESS: All checks were successful");
		}
	});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGES
//////////////////////////////////////////////////////////////////////

Task("TestPackages")
	.IsDependentOn("InstallPackages")
	.Does(() =>
	{
		foreach (var package in BuildSettings.Packages)
			if (package.IsNuGetPackage)
				new NuGetPackageTester(package).RunAllTests();
			else if (package.IsChocolateyPackage)
				new ChocolateyPackageTester(package).RunAllTests();
	});
*/
