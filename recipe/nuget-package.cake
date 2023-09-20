//////////////////////////////////////////////////////////////////////
// NUGET PACKAGE DEFINITION
//////////////////////////////////////////////////////////////////////

// Users may only instantiate the derived classes, which avoids
// exposing PackageType and makes it impossible to create a
// PackageDefinition with an unknown package type.
public class NuGetPackage : PackageDefinition
{
    /// <summary>
    /// Construct passing all provided arguments
    /// </summary>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName.</param>
    /// <param name="title">Title of the package for use in certain repositories.</param>
    /// <param name="description">A brief description of the package.</param>
    /// <param name="summary">A brief description of the package.</param>
    /// <param name="releaseNotes"></param>
    /// <param name="tags"></param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file. Either this or packageContent must be provided.</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="testRunner">A TestRunner instance used to run package tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
    /// <param name="preLoad">A collection of ExtensionSpecifiers to be preinstalled before running tests. Optional.</param>
	public NuGetPackage(
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
    : base (
        PackageType.NuGet, 
        id, 
        title: title,
        description: description,
        summary: summary,
        releaseNotes: releaseNotes,
        source: source,
        basePath: basePath,
        testRunner: testRunner,
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
    public override string PackageInstallDirectory => BuildSettings.NuGetTestDirectory;
    // The directory used to contain results of package tests for this package
    public override string PackageResultDirectory => BuildSettings.NuGetResultDirectory + PackageId + "/";
    // The directory into which extensions to the test runner are installed
    public override string ExtensionInstallDirectory => BuildSettings.PackageTestDirectory;

	protected virtual NuGetPackSettings NuGetPackSettings
    {
        get
        {
            var settings = new NuGetPackSettings
	        {
		        Id = PackageId,
                Version = PackageVersion,
                Title = PackageTitle ?? PackageId,
                // Deprecated by nuget: Summary = PackageSummary,
                Description = PackageDescription,
                ReleaseNotes = ReleaseNotes,
                Tags = Tags,
                Authors = TESTCENTRIC_PACKAGE_AUTHORS,
		        // Deprecated by nuget: Owners = TESTCENTRIC_PACKAGE_OWNERS,
		        Copyright = TESTCENTRIC_COPYRIGHT,
		        ProjectUrl = new Uri(TESTCENTRIC_PROJECT_URL),
		        License = TESTCENTRIC_LICENSE,
                Repository = new NuGetRepository() { Type="Git", Url="https://github.com/TestCentric/TestCentric.Cake.Recipe" },
		        RequireLicenseAcceptance = false,
		        // Deprecated by nuget: IconUrl = new Uri(TESTCENTRIC_ICON_URL),
		        Icon = TESTCENTRIC_ICON,
		        Language = "en-US",
                BasePath = BasePath,
		        Verbosity = BuildSettings.NuGetVerbosity,
                OutputDirectory = BuildSettings.PackagingDirectory,
                NoPackageAnalysis = true,
	        };

            if (SymbolChecks != null)
            {
                settings.Symbols = true;
                settings.SymbolPackageFormat = "snupkg";
            }

            return settings;
        }
    }

    public override void BuildPackage()
    {
        var settings = NuGetPackSettings;

        if (string.IsNullOrEmpty(PackageSource))
        {
            _context.Information("Using PackageSource");
            if (PackageContent != null)
            {
                foreach (var item in PackageContent.GetNuSpecContent())
                    settings.Files.Add(item);

                foreach (PackageReference dependency in PackageContent.Dependencies)
                    settings.Dependencies.Add(new NuSpecDependency { Id = dependency.Id, Version = dependency.Version } );
            }

            _context.NuGetPack(settings);
        }
        else switch(System.IO.Path.GetExtension(PackageSource))
        {
            case ".nuspec":
                _context.Information("Using Nuspec");
                _context.NuGetPack(PackageSource, settings);
                break;
            case ".csproj":
	            _context.Information("Using Csproj");
	            _context.MSBuild(PackageSource,
                    new MSBuildSettings {
                        Target = "pack",
		                Verbosity = BuildSettings.MSBuildVerbosity,
		                Configuration = BuildSettings.Configuration,
		                PlatformTarget = PlatformTarget.MSIL,
		                AllowPreviewVersion = BuildSettings.MSBuildAllowPreviewVersion
            	    }.WithProperty("Version", BuildSettings.PackageVersion));
                break;
            default:
                throw new ArgumentException(
                    $"Invalid package source specified: {PackageSource}", "source");
        }
    }

    protected override bool IsRemovableExtensionDirectory(DirectoryPath dirPath) =>
        dirPath.GetDirectoryName().StartsWith("NUnit.Extension.");
}
