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
    /// <param name="extraTestArguments>Additional arguments passed to the test runner.</param<
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
        IPackageTestRunner testRunner = null,
        TestRunnerSource testRunnerSource = null,
        string extraTestArguments = null,
        PackageCheck[] checks = null,
        PackageCheck[] symbols = null,
        IEnumerable<PackageTest> tests = null, 
        PackageReference[] preloadedExtensions = null,
        PackageContent packageContent = null )
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
        testRunnerSource: testRunnerSource,
        extraTestArguments: extraTestArguments,
        checks: checks,
        symbols: symbols,
        tests: tests,
        preloadedExtensions: preloadedExtensions,
        packageContent: packageContent )
    {
    }

    // The file name of this package, including extension
    public override string PackageFileName => $"{PackageId}.{PackageVersion}.nupkg";
    // The file name of any symbol package, including extension
    public override string SymbolPackageName => SIO.Path.ChangeExtension(PackageFileName, ".snupkg");
    // The directory into which this package and any extensions used are installed
    public override string PackageInstallDirectory => BuildSettings.PackageTestDirectory + PackageId + "/";

	protected virtual NuGetPackSettings NuGetPackSettings
    {
        get
        {
            var repositoryUrl = TESTCENTRIC_GITHUB_URL + BuildSettings.GitHubRepository + "/";
            var rawGitHubUserContent = "https://raw.githubusercontent.com/" + BuildSettings.GitHubRepository + "/main/";

            // NOTE: Because of how Cake build works, these settings will
            // override any settings in a nuspec file. Therefore, no settings
            // should be initialized unless they either
            //  1) are taken from the PackageDefinition itself.
            //  2) are taken from the BuildSettings, which apply to all packages being built.
            //  3) are defined to be the same for all TestCentric packages.

            var settings = new NuGetPackSettings
	        {
                // From PackageDefinition
		        Id = PackageId,
                Version = PackageVersion,
                Title = PackageTitle ?? PackageId,
                //Summary = PackageSummary, // Deprecated
                Description = PackageDescription,
                ReleaseNotes = ReleaseNotes,
                Tags = Tags,
                BasePath = BasePath,
                // From BuildSettings
		        Verbosity = BuildSettings.NuGetVerbosity,
                OutputDirectory = BuildSettings.PackageDirectory,
                Repository = new NuGetRepository() { Type="Git", Url=repositoryUrl },
                // Common to all packages
                Authors = TESTCENTRIC_PACKAGE_AUTHORS,
		        //Owners = TESTCENTRIC_PACKAGE_OWNERS, // Deprecated by NuGet
		        Copyright = TESTCENTRIC_COPYRIGHT,
		        ProjectUrl = new Uri(TESTCENTRIC_PROJECT_URL),
		        License = TESTCENTRIC_LICENSE,
		        RequireLicenseAcceptance = false,
		        //IconUrl = new Uri(TESTCENTRIC_ICON_URL), // Deprecated
		        Icon = TESTCENTRIC_ICON,
		        Language = "en-US",
                NoPackageAnalysis = true,
	        };

            if (SymbolChecks != null)
            {
                settings.Symbols = true;
                settings.SymbolPackageFormat = "snupkg";
            }

            if (PackageContent != null)
            {
                foreach (var item in PackageContent.GetNuSpecContent())
                    settings.Files.Add(item);

                foreach (PackageReference dependency in PackageContent.Dependencies)
                    settings.Dependencies.Add(new NuSpecDependency { Id = dependency.Id, Version = dependency.Version } );
            }

            return settings;
        }
    }

    public override void BuildPackage()
    {
        if (string.IsNullOrEmpty(PackageSource))
            _context.NuGetPack(NuGetPackSettings);
        else if (PackageSource.EndsWith(".nuspec"))
            _context.NuGetPack(PackageSource, NuGetPackSettings);
        else if (PackageSource.EndsWith(".csproj"))
            _context.MSBuild(PackageSource,
                new MSBuildSettings {
                    Target = "pack",
                    Verbosity = BuildSettings.MSBuildVerbosity,
                    Configuration = BuildSettings.Configuration,
                    PlatformTarget = PlatformTarget.MSIL,
                    AllowPreviewVersion = BuildSettings.MSBuildAllowPreviewVersion
                }.WithProperty("Version", BuildSettings.PackageVersion));
        else
            throw new ArgumentException(
                $"Invalid package source specified: {PackageSource}", "source");
    }

    protected override bool IsRemovableExtensionDirectory(DirectoryPath dirPath) =>
        dirPath.GetDirectoryName().StartsWith("NUnit.Extension.");
}
