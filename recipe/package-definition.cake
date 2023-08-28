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
    /// <param name="preloadedExtensions">An array of PackgaeReferences indicating extensions to be preinstalled before running tests. Optional.</param>
	protected PackageDefinition(
		PackageType packageType,
		string id,
        string title = null,
        string description = null,
        string summary = null,
        string[] releaseNotes = null,
        string[] tags = null,
		string source = null,
		string basePath = null,
        TestRunner testRunner = null,
		PackageCheck[] checks = null,
		PackageCheck[] symbols = null,
		IEnumerable<PackageTest> tests = null,
        PackageReference[] preloadedExtensions = null,
        PackageContent packageContent = null)
	{
        if (testRunner == null && tests != null)
            throw new System.ArgumentException($"Unable to create {packageType} package {id}: TestRunner must be provided if there are tests", nameof(testRunner));

        _context = BuildSettings.Context;

        PackageType = packageType;
		PackageId = id;
		PackageVersion = BuildSettings.PackageVersion;
        PackageTitle = title ?? id;
        PackageDescription = description ?? summary;
        PackageSummary = summary ?? description;
        ReleaseNotes = releaseNotes;
        Tags = tags ?? new [] { "testcentric" };
		PackageSource = source;
        BasePath = basePath ?? BuildSettings.OutputDirectory;
		TestRunner = testRunner;
		PackageChecks = checks;
		PackageTests = tests;
        PreLoadedExtensions = preloadedExtensions ?? new PackageReference[0];
		SymbolChecks = symbols;
        PackageContent = packageContent ?? new PackageContent();
	}

    public PackageType PackageType { get; }
	public string PackageId { get; }
	public string PackageVersion { get; }
    public string PackageTitle { get; }
    public string PackageSummary { get; }
    public string PackageDescription { get; }
    public string[] ReleaseNotes { get; }
    public string[] Tags { get; }
	public string PackageSource { get; }
    public string BasePath { get; }
    public TestRunner TestRunner { get; protected set; }
	public PackageCheck[] PackageChecks { get; protected set; }
    public PackageCheck[] SymbolChecks { get; protected set; }
    public IEnumerable<PackageTest> PackageTests { get; set; }
    public PackageReference[] PreLoadedExtensions { get; set; }
    public PackageContent PackageContent { get; }
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

    public void BuildVerifyAndTest()
    {
        _context.EnsureDirectoryExists(BuildSettings.PackageDirectory);

        Banner.Display($"Building {PackageFileName}");
        BuildPackage();

        Banner.Display($"Installing {PackageFileName}");
        InstallPackage();

        if (PackageChecks != null || PackageContent != null)
        {
            Banner.Display($"Verifying {PackageFileName}");
            VerifyPackage();
        }

        if (SymbolChecks != null)
        {
            // TODO: Override this in NuGetPackage
            VerifySymbolPackage();
        }

        if (PackageTests != null)
        {
            Banner.Display($"Testing {PackageFileName}");
            RunPackageTests();
        }
    }

    public abstract void BuildPackage();

    public abstract void InstallPackage();

    public void VerifyPackage()
    {
        bool allOK = true;

        if (PackageChecks != null)
            foreach (var check in PackageChecks)
                allOK &= check.ApplyTo(PackageInstallDirectory + PackageId);
        else // Use PackageContent
            allOK = PackageContent.VerifyInstallation(PackageInstallDirectory + PackageId);

        if (allOK)
            Console.WriteLine("All checks passed!");
        else 
            throw new Exception("Verification failed!");
    }

    public void RunPackageTests()
    {
        _context.Information($"Package tests will run at level {BuildSettings.PackageTestLevel}");

        var reporter = new ResultReporter(PackageFileName);

        _context.CleanDirectory(PackageResultDirectory);

		// Ensure we start out each package with no extensions installed.
		// If any package test installs an extension, it remains available
		// for subsequent tests of the same package only. 
		foreach (DirectoryPath dirPath in _context.GetDirectories(ExtensionInstallDirectory + "*"))
			if (IsRemovableExtensionDirectory(dirPath))
            {
		        _context.DeleteDirectory(dirPath, new DeleteDirectorySettings() { Recursive = true });
		        _context.Information("Deleted directory " + dirPath.GetDirectoryName());
            }

        // Pre-install any required extensions specified in BuildSettings.
        // Individual tests may still call for additional extensions.
        foreach(PackageReference package in PreLoadedExtensions)
            package.Install(ExtensionInstallDirectory);

        if (TestRunner.RequiresInstallation)
            TestRunner.Install();

        foreach (var packageTest in PackageTests)
        {
            if (packageTest.Level > BuildSettings.PackageTestLevel)
                continue;

            foreach (ExtensionSpecifier extension in packageTest.ExtensionsNeeded)
                extension.InstallExtension(this);

            var testResultDir = $"{PackageResultDirectory}/{packageTest.Name}/";
            var resultFile = testResultDir + "TestResult.xml";

            Banner.Display(packageTest.Description);

			_context.CreateDirectory(testResultDir);
            string arguments = packageTest.Arguments + $" --work={testResultDir}";
            if (BuildSettings.TraceLevel != "Off")
                arguments += $" --trace:{BuildSettings.TraceLevel}";

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

    public virtual void VerifySymbolPackage() { } // Does nothing. Overridden for NuGet packages.

    protected abstract bool IsRemovableExtensionDirectory(DirectoryPath dirPath);
}
