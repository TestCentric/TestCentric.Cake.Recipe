//////////////////////////////////////////////////////////////////////
// CHOCOLATEY PACKAGE DEFINITION
//////////////////////////////////////////////////////////////////////

public class ChocolateyPackage : PackageDefinition
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
	public ChocolateyPackage(
        string id, string source, string basePath, TestRunner testRunner = null,
        PackageCheck[] checks = null, PackageCheck[] symbols = null, IEnumerable<PackageTest> tests = null, IEnumerable<PackageSpecifier> preloadedExtensions = null)
      : base (PackageType.Chocolatey, id, source, basePath, testRunner: testRunner, checks: checks, symbols: symbols, tests: tests, preloadedExtensions: preloadedExtensions)
    {
    }

    // The file name of this package, including extension
    public override string PackageFileName => $"{PackageId}.{PackageVersion}.nupkg";
    // The file name of any symbol package, including extension
    public override string SymbolPackageName => System.IO.Path.ChangeExtension(PackageFileName, ".snupkg");
    // The directory into which this package is installed
    public override string PackageInstallDirectory => BuildSettings.ChocolateyTestDirectory;
    // The directory used to contain results of package tests for this package
    public override string PackageResultDirectory => BuildSettings.ChocolateyResultDirectory + PackageId + "/";
    // The directory into which extensions to the test runner are installed
    public override string ExtensionInstallDirectory => BuildSettings.PackageTestDirectory;

    protected override void doBuildPackage()
    {
        var chocolateyPackSettings = new ChocolateyPackSettings()
        {
            Version = PackageVersion,
            OutputDirectory = BuildSettings.PackageDirectory,
            ArgumentCustomization = args => args.Append($"BIN={BasePath}")
        };

        _context.ChocolateyPack(PackageSource, chocolateyPackSettings);
    }
}
