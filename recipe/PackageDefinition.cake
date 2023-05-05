//////////////////////////////////////////////////////////////////////
// PACKAGE DEFINITION ABSTRACT CLASS
//////////////////////////////////////////////////////////////////////

public enum PackageType
{
	NuGet,
	Chocolatey,
	Msi,
	Zip
}

public abstract class PackageDefinition
{
    protected ICakeContext _context;

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
    /// <param name="tests">An collection of PackageTests to be run against the package. Optional.</param>
    /// <param name="preload">A collection of ExtensionSpecifiers to be preinstalled before running tests. Optional.</param>
	protected PackageDefinition(
		PackageType packageType,
		string id,
		string source,
		string basePath,
        TestRunner testRunner = null,
		PackageCheck[] checks = null,
		PackageCheck[] symbols = null,
		IEnumerable<PackageTest> tests = null,
        IEnumerable<PackageSpecifier> preload = null)
	{
        if (testRunner == null && tests != null)
            throw new System.ArgumentException($"Unable to create {packageType} package {id}: TestRunner must be provided if there are tests", nameof(testRunner));

        _context = BuildSettings.Context;

        PackageType = packageType;
		PackageId = id;
		PackageVersion = BuildSettings.PackageVersion;
		PackageSource = source;
		BasePath = basePath;
		TestRunner = testRunner;
		PackageChecks = checks;
		PackageTests = tests;
        PreLoadedExtensions = preload;
		SymbolChecks = symbols;
	}

    public PackageType PackageType { get; }
	public string PackageId { get; }
	public string PackageVersion { get; }
	public string PackageSource { get; }
    public string BasePath { get; protected set; }
    public TestRunner TestRunner { get; protected set; }
	public PackageCheck[] PackageChecks { get; protected set; }
    public PackageCheck[] SymbolChecks { get; protected set; }
    public IEnumerable<PackageTest> PackageTests { get; set; }
    public IEnumerable<PackageSpecifier> PreLoadedExtensions { get; set; }
    public bool HasTests => PackageTests != null;
    public bool HasChecks => PackageChecks != null;
    public bool HasSymbols => SymbolChecks != null;
    public virtual string SymbolPackageName => throw new System.NotImplementedException($"Symbols are not available for this type of package.");

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
        // Target Package is in package directory but may have dependencies
		var packageSources = new []
		{
            BuildSettings.PackageDirectory,
			"https://www.myget.org/F/testcentric/api/v3/index.json",
			PackageType == PackageType.Chocolatey
				? "https://community.chocolatey.org/api/v2/"
				: "https://api.nuget.org/v3/index.json"
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

    public void VerifyPackage()
    {
        DisplayAction("Verifying");
        Console.WriteLine($"Base Directory: {PackageInstallDirectory + PackageId}");

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
        _context.Information($"Package tests will run at level {BuildSettings.PackageTestLevel}");

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

        // Pre-install any required extensions specified in BuildSettings.
        // Individual tests may still call for additional extensions.
        foreach(PackageSpecifier package in PreLoadedExtensions)
            package.Install(ExtensionInstallDirectory);

        foreach (var packageTest in PackageTests)
        {
            if (packageTest.Level > BuildSettings.PackageTestLevel)
                continue;

            foreach (ExtensionSpecifier extensionSpecifier in packageTest.ExtensionsNeeded)
            {
                PackageSpecifier package = PackageType == PackageType.Chocolatey
                    ? extensionSpecifier.ChocoPackage
                    : extensionSpecifier.NuGetPackage;
                
                package.Install(ExtensionInstallDirectory);
            }

            var testResultDir = $"{PackageResultDirectory}/{packageTest.Name}/";
            var resultFile = testResultDir + "TestResult.xml";

            Banner.Display(packageTest.Description);
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
        Banner.Display($"{action} package {PackageFileName}");
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
}
