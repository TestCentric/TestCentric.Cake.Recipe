//////////////////////////////////////////////////////////////////////
// PACKAGE DEFINITION ABSTRACT CLASS
//////////////////////////////////////////////////////////////////////

public enum PackageType
{
	NuGet,
	Chocolatey,
	Zip
}

public abstract class PackageDefinition
{
    protected ICakeContext _context;

	/// <summary>
    /// Constructor
    /// </summary>
    /// <param name="packageType">A PackageType value specifying one of the four known package types</param>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName</param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="testRunner">A TestRunner instance used to run package tests.</param>
    /// <param name="testRunnerSource">A TestRunnerSource instance used to provide runners for package tests.</param>
    /// <param name="extraTestArguments>Additional arguments passed to the test runner.</param<
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">A collection of PackageTests to be run against the package. Optional.</param>
    /// <param name="preloadedExtensions">An array of PackgaeReferences indicating extensions to be preinstalled before running tests. Optional.</param>
	protected PackageDefinition(
		PackageType packageType,
		string id,
		string source = null,
		string basePath = null, // Defaults to OutputDirectory
        string title = null,
        string description = null,
        string summary = null,
        string[] releaseNotes = null,
        string[] tags = null,
        IPackageTestRunner testRunner = null,
        TestRunnerSource testRunnerSource = null,
        string extraTestArguments = null,
		PackageCheck[] checks = null,
		PackageCheck[] symbols = null,
		IEnumerable<PackageTest> tests = null,
        PackageReference[] preloadedExtensions = null,
        PackageContent packageContent = null)
	{
        if (testRunner == null && testRunnerSource == null && tests != null)
            throw new System.InvalidOperationException($"Unable to create {packageType} package {id}: TestRunner or TestRunnerSource must be provided if there are tests.");
        if (testRunner != null && testRunnerSource != null)
            throw new System.InvalidOperationException($"Unable to create {packageType} package {id}: Either TestRunner or TestRunnerSource must be provided, but not both.");

        _context = BuildSettings.Context;

        PackageType = packageType;
		PackageId = id;
		PackageVersion = BuildSettings.PackageVersion;
		PackageSource = source;
        BasePath = basePath ?? BuildSettings.OutputDirectory;
        PackageTitle = title ?? id;
        PackageDescription = description ?? summary;
        PackageSummary = summary ?? description;
        ReleaseNotes = releaseNotes;
        Tags = tags ?? new [] { "testcentric" };
		TestRunner = testRunner;
        TestRunnerSource = testRunnerSource;
        ExtraTestArguments = extraTestArguments;
		PackageChecks = checks;
		SymbolChecks = symbols;
		PackageTests = tests;
        PreLoadedExtensions = preloadedExtensions ?? new PackageReference[0];
        PackageContent = packageContent ?? new PackageContent();
	}

    public PackageType PackageType { get; }
	public string PackageId { get; }
	public string PackageVersion { get; protected set; }
	public string PackageSource { get; }
    public string BasePath { get; }
    public IPackageTestRunner TestRunner { get; protected set; }
    public TestRunnerSource TestRunnerSource { get; }
    public string PackageTitle { get; }
    public string PackageSummary { get; }
    public string PackageDescription { get; }
    public string[] ReleaseNotes { get; }
    public string[] Tags { get; }
    public string ExtraTestArguments {get; protected set; }
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
    public string PackageTestDirectory => $"{PackageInstallDirectory}{PackageId}.{PackageVersion}/";

    public bool IsSelectedBy(string selectionExpression)
    {
        return IsSelectedByAny(selectionExpression.Split("|", StringSplitOptions.RemoveEmptyEntries));

        bool IsSelectedByAny(string[] terms)
        {
            foreach (var term in terms)
                if (IsSelectedByAll(term.Split("&", StringSplitOptions.RemoveEmptyEntries)))
                    return true;

            return false;
        }

        bool IsSelectedByAll(string[] factors)
        {
            foreach (string factor in factors)
            {
                int index = factor.IndexOf("=");
                if (index <= 0)
                    throw new ArgumentException("Selection expression does not contain =", "where");
                string prop = factor.Substring(0, index).Trim();
                string val = factor.Substring(index+1).Trim();

                switch(prop.ToUpper())
                {
                    case "ID":
                        return PackageId.ToLower() == val.ToLower();
                    case "TYPE":
                        return PackageType.ToString().ToLower() == val.ToLower();
                    default:
                        throw new Exception($"Not a valid selection property: {prop}");
                }
            }

            return false;
        }
    }

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

    // This may be called by NuGet or Chocolatey packages
    public void AddPackageToLocalFeed()
    {
		try
		{
			_context.NuGetAdd(PackageFilePath, BuildSettings.LocalPackagesDirectory);
		}
		catch (Exception ex)
		{
			_context.Error(ex.Message);
		}
    }

    // Base implementation is used for installing both NuGet and
    // Chocolatey packages. Other package types should override.
    public virtual void InstallPackage()
    {
	    var installSettings = new NuGetInstallSettings
	    {
		    Source = new [] {
                // Package will be found here
                BuildSettings.PackageDirectory,
                // Dependencies may be in any of these
                BuildSettings.LocalPackagesDirectory,
			    "https://www.myget.org/F/testcentric/api/v3/index.json",
			    "https://api.nuget.org/v3/index.json" },
            Version = PackageVersion,
            OutputDirectory = PackageInstallDirectory,
            //ExcludeVersion = true,
		    Prerelease = true,
		    Verbosity = BuildSettings.NuGetVerbosity
	    }
        .WithArgumentCustomization(args => args.Append("/NoHttpCache"));

        _context.NuGetInstall(PackageId, installSettings);
    }

    public void VerifyPackage()
    {
        bool allOK = true;

        if (PackageChecks != null)
            foreach (var check in PackageChecks)
                allOK &= check.ApplyTo(PackageTestDirectory);
        else // Use PackageContent
            allOK = PackageContent.VerifyInstallation(PackageTestDirectory);

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

        // Package was defined with either a TestRunnerSource or a single TestRunner. In either
        // case, these will all be package test runners and may or may not require installation.
        var defaultRunners = TestRunnerSource ?? new TestRunnerSource((TestRunner)TestRunner);

        // Preinstall all runners requiring installation
        InstallRunners(defaultRunners.PackageTestRunners);

        foreach (var packageTest in PackageTests)
        {
            if (packageTest.Level > BuildSettings.PackageTestLevel)
                continue;

            InstallExtensions(packageTest.ExtensionsNeeded);
            InstallRunners(packageTest.TestRunners);

            var testResultDir = $"{PackageResultDirectory}/{packageTest.Name}/";
            var resultFile = testResultDir + "TestResult.xml";

            Banner.Display(packageTest.Description);

			_context.CreateDirectory(testResultDir);
            string arguments = $"{packageTest.Arguments} {ExtraTestArguments} --work={testResultDir}";
            if (CommandLineOptions.TraceLevel.Value != "Off")
                arguments += $" --trace:{CommandLineOptions.TraceLevel.Value}";

            int rc = TestRunner.RunPackageTest(arguments);

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

        bool hadErrors = reporter.ReportResults(Console.Out);
        Console.WriteLine();

        if (hadErrors)
            throw new Exception("One or more package tests had errors!");
    }

    private void InstallExtensions(ExtensionSpecifier[] extensionsNeeded)
    {
        foreach (ExtensionSpecifier extension in extensionsNeeded)
            extension.InstallExtension(this);
    }

    private void InstallRunners(IEnumerable<IPackageTestRunner> runners)
    {
        // Install any runners needing installation
        foreach (var runner in runners)
            if (runner is InstallableTestRunner)
                InstallRunner((InstallableTestRunner)runner);
    }

    private void InstallRunner(InstallableTestRunner runner)
    {
        runner.Install(PackageInstallDirectory);

		// We are using nuget packages for the runner, so it won't normally recognize
		// chocolatey extensions. We add an extra addins file for that purpose.
        if (PackageType == PackageType.Chocolatey)
        {
            var filePath = runner.ExecutablePath.GetDirectory().CombineWithFilePath("choco.engine.addins").ToString();
            Console.WriteLine($"Creating {filePath}");

			using (var writer = new StreamWriter(filePath))
			{
				writer.WriteLine("../../nunit-extension-*/tools/");
				writer.WriteLine("../../nunit-extension-*/tools/*/");
				writer.WriteLine("../../../nunit-extension-*/tools/");
				writer.WriteLine("../../../nunit-extension-*/tools/*/");
			}
        }
    }

public virtual void VerifySymbolPackage() { } // Does nothing. Overridden for NuGet packages.

    protected abstract bool IsRemovableExtensionDirectory(DirectoryPath dirPath);
}
