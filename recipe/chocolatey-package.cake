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
    /// <param name="extraTestArguments>Additional arguments passed to the test runner.</param<
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
	public ChocolateyPackage(
        string id, 
        string title = null,
        string summary = null,
        string description = null,
        string[] releaseNotes = null,
        string[] tags = null,
        string source = null, 
        string basePath = null,
        TestRunner testRunner = null,
        string extraTestArguments = null,
        PackageCheck[] checks = null,
        PackageCheck[] symbols = null,
        IEnumerable<PackageTest> tests = null,
        PackageReference[] preloadedExtensions = null,
        PackageContent packageContent = null)
    : base (
        PackageType.Chocolatey,
        id, 
        title: title,
        summary: summary,
        description: description,
        releaseNotes: releaseNotes,
        tags: tags,
        source: source,
        basePath: basePath,
        testRunner: testRunner,
        extraTestArguments: extraTestArguments,
        checks: checks,
        symbols: symbols,
        tests: tests,
        preloadedExtensions: preloadedExtensions,
        packageContent: packageContent)
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

    protected virtual ChocolateyPackSettings ChocolateyPackSettings
    {
        get
        {
            var settings = new ChocolateyPackSettings
            {
		        Id = PackageId,
                Version = PackageVersion,
                Title = PackageTitle ?? PackageId,
                Summary = PackageSummary,
                Description = PackageDescription,
                ReleaseNotes = ReleaseNotes,
                Tags = Tags,
                Authors = TESTCENTRIC_PACKAGE_AUTHORS,
		        Owners = TESTCENTRIC_PACKAGE_OWNERS,
		        Copyright = TESTCENTRIC_COPYRIGHT,
		        ProjectUrl = new Uri(TESTCENTRIC_PROJECT_URL),
		        LicenseUrl = new Uri(TESTCENTRIC_LICENSE_URL),
		        RequireLicenseAcceptance = false,
		        IconUrl = new Uri(TESTCENTRIC_ICON_URL),
                ProjectSourceUrl = new Uri(PROJECT_REPOSITORY_URL),
                PackageSourceUrl = new Uri(PROJECT_REPOSITORY_URL),
                DocsUrl = new Uri(TESTCENTRIC_PROJECT_URL),
                MailingListUrl = new Uri(TESTCENTRIC_MAILING_LIST_URL),
                BugTrackerUrl = new Uri(PROJECT_REPOSITORY_URL + "issues"),
		        Verbose = BuildSettings.ChocolateyVerbosity,
                OutputDirectory = BuildSettings.PackagingDirectory,
	        };

            if (PackageContent != null)
            {
                foreach (var item in PackageContent.GetChocolateyNuSpecContent(BasePath))
                    settings.Files.Add(item);

                foreach (PackageReference dependency in PackageContent.Dependencies)
                    settings.Dependencies.Add(new ChocolateyNuSpecDependency { Id = dependency.Id, Version = dependency.Version } );
            }

            return settings;
        }
    }

    public override void BuildPackage()
    {
        if (string.IsNullOrEmpty(PackageSource))
            _context.ChocolateyPack(ChocolateyPackSettings);
        else if (PackageSource.EndsWith(".nuspec"))
            _context.ChocolateyPack(PackageSource, ChocolateyPackSettings);
        else
            throw new ArgumentException(
                $"Invalid package source specified: {PackageSource}", "source");
    }

    protected override bool IsRemovableExtensionDirectory(DirectoryPath dirPath) =>
        dirPath.GetDirectoryName().StartsWith("nunit-extension-");
}
