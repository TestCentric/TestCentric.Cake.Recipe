//////////////////////////////////////////////////////////////////////
// PACKAGING TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageChocolatey");

Task("PackageNuGet")
	.IsDependentOn("BuildNuGetPackage")
	.IsDependentOn("VerifyNuGetPackage")
	.IsDependentOn("TestNuGetPackage");

Task("PackageChocolatey")
	.IsDependentOn("BuildChocolateyPackage")
	.IsDependentOn("VerifyChocolateyPackage")
	.IsDependentOn("TestChocolateyPackage");

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGES
//////////////////////////////////////////////////////////////////////

Task("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
			package.BuildPackage();
	});

Task("BuildNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		CreateDirectory(settings.PackageDirectory);

		NuGetPack(settings.NuGetPackageSource, new NuGetPackSettings()
		{
			Version = settings.PackageVersion,
			OutputDirectory = settings.PackageDirectory,
			NoPackageAnalysis = true
		});
	});

Task("BuildChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		CreateDirectory(settings.PackageDirectory);

		ChocolateyPack(settings.ChocolateyPackageSource, new ChocolateyPackSettings()
		{
			Version = settings.PackageVersion,
			OutputDirectory = settings.PackageDirectory
		});
	});

//////////////////////////////////////////////////////////////////////
// INSTALL PACKAGES
//////////////////////////////////////////////////////////////////////

Task("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		if (System.IO.Directory.Exists(settings.NuGetTestDirectory))
			DeleteDirectory(settings.NuGetTestDirectory,
				new DeleteDirectorySettings()
				{
					Recursive = true
				});

		CreateDirectory(settings.NuGetTestDirectory);

		Unzip(settings.NuGetPackage, settings.NuGetTestDirectory);

		Information($"  Installed {System.IO.Path.GetFileName(settings.NuGetPackage)}");
		Information($"    at {settings.NuGetTestDirectory}");
	});

Task("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		if (System.IO.Directory.Exists(settings.ChocolateyTestDirectory))
			DeleteDirectory(settings.ChocolateyTestDirectory,
				new DeleteDirectorySettings()
				{
					Recursive = true
				});

		CreateDirectory(settings.ChocolateyTestDirectory);

		Unzip(settings.ChocolateyPackage, settings.ChocolateyTestDirectory);

		Information($"  Installed {System.IO.Path.GetFileName(settings.ChocolateyPackage)}");
		Information($"    at {settings.ChocolateyTestDirectory}");
	});

//////////////////////////////////////////////////////////////////////
// CHECK PACKAGE CONTENT
//////////////////////////////////////////////////////////////////////

Task("VerifyPackages")
	.IsDependentOn("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			Check.That(package.PackageTestDirectory, package.PackageChecks);
			Information("  SUCCESS: All checks were successful");
		}
	});

// TODO: Move specific checks to build.cake
static readonly string[] LAUNCHER_FILES = {
	"net20-agent-launcher.dll", "nunit.engine.api.dll"
};

static readonly string[] AGENT_FILES = {
	"net20-pluggable-agent.exe", "net20-pluggable-agent.exe.config",
	"net20-pluggable-agent-x86.exe", "net20-pluggable-agent-x86.exe.config",
	"nunit.engine.api.dll", "testcentric.engine.core.dll"
};

Task("VerifyNuGetPackage")
	.IsDependentOn("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		Check.That(settings.NuGetTestDirectory,
		HasFiles("LICENSE.txt", "CHANGES.txt"),
			HasDirectory("tools").WithFiles(LAUNCHER_FILES),
			HasDirectory("tools/agent").WithFiles(AGENT_FILES));

		Information("  SUCCESS: All checks were successful");
	});

Task("VerifyChocolateyPackage")
	.IsDependentOn("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		Check.That(settings.ChocolateyTestDirectory,
			HasDirectory("tools").WithFiles("LICENSE.txt", "CHANGES.txt", "VERIFICATION.txt").WithFiles(LAUNCHER_FILES),
			HasDirectory("tools/agent").WithFiles(AGENT_FILES));

		Information("  SUCCESS: All checks were successful");
	});

//////////////////////////////////////////////////////////////////////
// TEST PACKAGES
//////////////////////////////////////////////////////////////////////

Task("TestPackages")
	.IsDependentOn("BuildPackages")
	.Does<BuildSettings>((settings) =>
	{
		foreach (var package in settings.Packages)
		{
			package.TestPackage();
		}
	});

Task("TestNuGetPackage")
	.IsDependentOn("InstallNuGetPackage")
	.Does<BuildSettings>((settings) =>
	{
		new NuGetPackageTester(settings).RunAllTests();
	});

Task("TestChocolateyPackage")
	.IsDependentOn("InstallChocolateyPackage")
	.Does<BuildSettings>((settings) =>
	{
		new ChocolateyPackageTester(settings).RunAllTests();
	});
