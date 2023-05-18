//////////////////////////////////////////////////////////////////////
// NUGET PACKAGE DEFINITION
//////////////////////////////////////////////////////////////////////

// Users may only instantiate the derived classes, which avoids
// exposing PackageType and makes it impossible to create a
// PackageDefinition with an unknown package type.
public class NuGetPackage : PackageDefinition
{
    /// <summary>
    /// Construct passing all required arguments
    /// </summary>
    /// <param name="packageType">A PackageType value specifying one of the four known package types</param>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName</param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="testRunner">A TestRunner instance used to run package tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
    /// <param name="preLoad">A collection of ExtensionSpecifiers to be preinstalled before running tests. Optional.</param>
	public NuGetPackage(
        string id, string source, string basePath, TestRunner testRunner = null,
        PackageCheck[] checks = null, PackageCheck[] symbols = null,
        IEnumerable<PackageTest> tests = null, 
        ExtensionSpecifier[] preloadedExtensions = null)
      : base (PackageType.NuGet, id, source, basePath, testRunner: testRunner,
        checks: checks, symbols: symbols, tests: tests, preloadedExtensions: preloadedExtensions)
    {
    }

    // The file name of this package, including extension
    public override string PackageFileName => $"{PackageId}.{PackageVersion}.nupkg";
    // The file name of any symbol package, including extension
    public override string SymbolPackageName => System.IO.Path.ChangeExtension(PackageFileName, ".snupkg");
    // The directory into which this package is installed
    public override string PackageInstallDirectory => BuildSettings.NuGetTestDirectory;
    // The directory used to contain results of package tests for this package
    public override string PackageResultDirectory => BuildSettings.NuGetResultDirectory + PackageId + "/";
    // The directory into which extensions to the test runner are installed
    public override string ExtensionInstallDirectory => BuildSettings.PackageTestDirectory;

	public NuGetPackSettings DefaultPackSettings()
    {
        var settings = new NuGetPackSettings
	    {
		    Id = PackageId,
            Version = PackageVersion,
            Authors = TESTCENTRIC_AUTHORS,
		    Owners = TESTCENTRIC_OWNERS,
		    Copyright =TESTCENTRIC_COPYRIGHT,
		    ProjectUrl = new Uri(TESTCENTRIC_PROJECT_URL),
		    License = TESTCENTRIC_LICENSE,
		    RequireLicenseAcceptance = false,
		    IconUrl = new Uri(TESTCENTRIC_ICON_URL),
		    Icon = TESTCENTRIC_ICON,
		    Language = "en-US",
            BasePath = BasePath,
            Symbols = HasSymbols,
		    Verbosity = BuildSettings.NuGetVerbosity,
            OutputDirectory = BuildSettings.PackageDirectory,
            NoPackageAnalysis = true
	    };

        if (HasSymbols)
            settings.SymbolPackageFormat = "snupkg";

        return settings;
    }

    public override void BuildPackage()
    {
        _context.NuGetPack(PackageSource, DefaultPackSettings());
    }

    public override void InstallPackage()
    {
        // Target Package is in package directory but may have dependencies
		var packageSources = new []
		{
            BuildSettings.PackageDirectory,
			"https://www.myget.org/F/testcentric/api/v3/index.json",
			"https://api.nuget.org/v3/index.json"
		};

        // Install using nuget to avoid need for admin level
        _context.NuGetInstall(PackageId, new NuGetInstallSettings
        {
            Source = packageSources,
            Version = PackageVersion,
            Prerelease = true,
            Verbosity = BuildSettings.NuGetVerbosity,
            NoCache = true,
            OutputDirectory = PackageInstallDirectory,
            ExcludeVersion = true
        });
    }
}
