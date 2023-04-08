//////////////////////////////////////////////////////////////////////
// PACKAGE DEFINITION ABSTRACT CLASS
//////////////////////////////////////////////////////////////////////

public abstract class PackageDefinition
{
	protected enum PackageType
	{
		NuGet,
		Chocolatey,
		Msi,
		Zip
	}

    protected ICakeContext _context;
    private PackageType _packageType;

	/// <summary>
    /// Constructor
    /// </summary>
    /// <param name="settings">An instance of BuildSettings</param>
    /// <param name="packageType">A PackageType value specifying one of the four known package types</param>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName</param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="testRunner">A TestRunner instance used to run package tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
	protected PackageDefinition(
		PackageType packageType,
		string id,
		string source,
		string basePath,
        TestRunner testRunner = null,
		PackageCheck[] checks = null,
		PackageCheck[] symbols = null,
		IEnumerable<PackageTest> tests = null)
	{
        if (testRunner == null && tests != null)
            throw new System.ArgumentException($"Unable to create {packageType} package {id}: TestRunner must be provided if there are tests", nameof(testRunner));

        _context = BuildSettings.Context;
        _packageType = packageType;

		PackageId = id;
		PackageVersion = BuildSettings.PackageVersion;
		PackageSource = source;
		BasePath = basePath;
		TestRunner = testRunner;
		PackageChecks = checks;
		PackageTests = tests;
		SymbolChecks = symbols;
	}

	public string PackageId { get; }
	public string PackageVersion { get; }
	public string PackageSource { get; }
    public string BasePath { get; protected set; }
    public TestRunner TestRunner { get; protected set; }
	public PackageCheck[] PackageChecks { get; }
    public PackageCheck[] SymbolChecks { get; protected set; }
    public IEnumerable<PackageTest> PackageTests { get; set; }
    public bool HasTests => PackageTests != null;
    public bool HasChecks => PackageChecks != null;
    public bool HasSymbols => SymbolChecks != null;
    public virtual string SymbolPackageName => throw new System.NotImplementedException($"Symbols are not available for this type of package.");

	public bool IsNuGetPackage => _packageType == PackageType.NuGet;
	public bool IsChocolateyPackage => _packageType == PackageType.Chocolatey;

    // The file name of this package, including extension
    public abstract string PackageFileName { get; }
    // The directory into which this package is installed
    public abstract string PackageInstallDirectory { get; }
    // The directory used to contain results of package tests for this package
    public abstract string PackageResultDirectory { get; }
    // The directory into which extensions to the test runner are installed
    public abstract string ExtensionInstallDirectory { get; }

    public string PackageFilePath => BuildSettings.PackageDirectory + PackageFileName;

    protected abstract void doBuildPackage();

    public void BuildVerifyAndTest()
    {
        _context.EnsureDirectoryExists(BuildSettings.PackageDirectory);

        BuildPackage();
        InstallPackage();

        if (HasChecks)
            VerifyPackage();

        if (HasSymbols)
            VerifySymbolPackage();

        if (HasTests)
            RunPackageTests();
    }

    public void BuildPackage()
    {
        DisplayAction("Building");
        doBuildPackage();
    }

    public void InstallPackage()
    {
        DisplayAction("Installing");
        Console.WriteLine($"Installing package to {PackageInstallDirectory}");
        _context.CleanDirectory(PackageInstallDirectory + PackageId);
        doInstallPackage();
    }

    protected virtual void doInstallPackage()
    {
        // Install using nuget to avoid need for admin level
        _context.NuGetInstall(PackageId, new NuGetInstallSettings
        {
            Source = new[] { BuildSettings.PackageDirectory },
            Prerelease = true,
            NoCache = true,
            OutputDirectory = PackageInstallDirectory,
            ExcludeVersion = true
        });
    }

    public void VerifyPackage()
    {
        DisplayAction("Verifying");

        bool allOK = true;
        foreach (var check in PackageChecks)
            allOK &= check.ApplyTo(PackageInstallDirectory + PackageId);

        if (allOK)
            Console.WriteLine("All checks passed!");
        else 
            throw new Exception("Verification failed!");
    }

    public void RunPackageTests()
    {
        DisplayAction("Testing");

        var reporter = new ResultReporter(PackageFileName);

        _context.CleanDirectory(PackageResultDirectory);

		// Ensure we start out each package with no extensions installed.
		// If any package test installs an extension, it remains available
		// for subsequent tests of the same package only.
		foreach (var dirPath in _context.GetDirectories(ExtensionInstallDirectory + "*"))
		{
			string dirName = dirPath.GetDirectoryName();
			if (dirName.StartsWith("NUnit.Extension.") || dirName.StartsWith("nunit-extension-"))
			{
				_context.DeleteDirectory(dirPath, new DeleteDirectorySettings() { Recursive = true });
				Console.WriteLine("Deleted directory " + dirName);
			}
		}

        foreach (var packageTest in PackageTests)
        {
            foreach (ExtensionSpecifier extension in packageTest.ExtensionsNeeded)
                CheckExtensionIsInstalled(extension);

            var testResultDir = $"{PackageResultDirectory}/{packageTest.Name}/";
            var resultFile = testResultDir + "TestResult.xml";

            DisplayBanner(packageTest.Description);
			DisplayTestEnvironment(packageTest);

			_context.CreateDirectory(testResultDir);
            string arguments = packageTest.Arguments + $" --work={testResultDir}";

            int rc = TestRunner.Run(arguments);

            try
            {
                var result = new ActualResult(resultFile);
                var report = new PackageTestReport(packageTest, result);
                reporter.AddReport(report);

                Console.WriteLine(report.Errors.Count == 0
                    ? "\nSUCCESS: Test Result matches expected result!"
                    : "\nERROR: Test Result not as expected!");
            }
            catch (Exception ex)
            {
                reporter.AddReport(new PackageTestReport(packageTest, ex));

                Console.WriteLine("\nERROR: No result found!");
            }
        }

        bool hadErrors = reporter.ReportResults();
        Console.WriteLine();

        if (hadErrors)
            throw new Exception("One or more package tests had errors!");
    }

    public void DisplayAction(string action)
    {
        DisplayBanner($"{action} package {PackageFileName}");
    }

	private void DisplayTestEnvironment(PackageTest test)
	{
		Console.WriteLine("Test Environment");
		Console.WriteLine($"   OS Version: {Environment.OSVersion.VersionString}");
		Console.WriteLine($"  CLR Version: {Environment.Version}");
		Console.WriteLine($"    Arguments: {test.Arguments}");
		Console.WriteLine();
	}

    public virtual void VerifySymbolPackage() { } // Does nothing. Overridden for NuGet packages.

    private void CheckExtensionIsInstalled(ExtensionSpecifier extension)
    {
        string extensionId = _packageType == PackageType.Chocolatey ? extension.ChocoId : extension.NuGetId;

        bool alreadyInstalled = _context.GetDirectories($"{ExtensionInstallDirectory}{extensionId}.*").Count > 0;

        if (!alreadyInstalled)
        {
            DisplayBanner($"Installing {extensionId} version {extension.Version}");

            _context.NuGetInstall(extensionId,
                new NuGetInstallSettings()
                {
                    OutputDirectory = ExtensionInstallDirectory,
                    Version = extension.Version
                });
        }
    }
}

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
    /// <param name="testRunner">A TestRunner instance used to run packae tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
	public NuGetPackage(
        string id, string source, string basePath, TestRunner testRunner = null,
        PackageCheck[] checks = null, PackageCheck[] symbols = null, IEnumerable<PackageTest> tests = null)
      : base (PackageType.NuGet, id, source, basePath, testRunner: testRunner, checks: checks, symbols: symbols, tests: tests)
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

    protected override void doBuildPackage()
    {
        var nugetPackSettings = new NuGetPackSettings()
        {
            Version = PackageVersion,
            OutputDirectory = BuildSettings.PackageDirectory,
            BasePath = BasePath,
            NoPackageAnalysis = true,
            Symbols = HasSymbols
        };

        if (HasSymbols)
            nugetPackSettings.SymbolPackageFormat = "snupkg";

        _context.NuGetPack(PackageSource, nugetPackSettings);
    }
}

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
    /// <param name="testRunner">A TestRunner instance used to run packae tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
	public ChocolateyPackage(
        string id, string source, string basePath, TestRunner testRunner = null,
        PackageCheck[] checks = null, PackageCheck[] symbols = null, IEnumerable<PackageTest> tests = null)
      : base (PackageType.Chocolatey, id, source, basePath, testRunner: testRunner, checks: checks, symbols: symbols, tests: tests)
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
