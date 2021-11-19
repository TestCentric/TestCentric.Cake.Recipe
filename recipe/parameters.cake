#load "./check-headers.cake"
#load "./package-checks.cake"
#load "./test-results.cake"
#load "./test-report.cake"
#load "./package-tests.cake"
#load "./testcentric-gui.cake"
#load "./versioning.cake"

// URLs for uploading packages
private const string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";
private const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
private const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

// Environment Variable names holding API keys
private const string MYGET_API_KEY = "MYGET_API_KEY";
private const string NUGET_API_KEY = "NUGET_API_KEY";
private const string CHOCO_API_KEY = "CHOCO_API_KEY";
private const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";

// Pre-release labels that we publish
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };

// Defaults
const string DEFAULT_CONFIGURATION = "Release";

// Standard Header. Each string represents one line.
static readonly string[] DEFAULT_STANDARD_HEADER = new[] {
    "// ***********************************************************************",
    "// Copyright (c) Charlie Poole and TestCentric Engine contributors.",
    "// Licensed under the MIT License. See LICENSE.txt in root directory.",
    "// ***********************************************************************"
};

public class BuildParameters
{
    private ISetupContext _context;

	public BuildParameters(ISetupContext context)
    {
        _context = context;

		Configuration = _context.Argument("configuration", DEFAULT_CONFIGURATION);
		ProjectDirectory = _context.Environment.WorkingDirectory.FullPath + "/";

		BuildVersion = new BuildVersion(context);
    }

    public ISetupContext Context => _context;

	// Arguments
	public string Configuration { get; }

	// Versioning
	public BuildVersion BuildVersion { get; }
	public string PackageVersion => BuildVersion.PackageVersion;
	public string AssemblyVersion => BuildVersion.AssemblyVersion;
	public string AssemblyFileVersion => BuildVersion.AssemblyFileVersion;
	public string AssemblyInformationalVersion => BuildVersion.AssemblyInformationalVersion;
	public bool IsProductionRelease => !PackageVersion.Contains("-");
	public bool IsDevelopmentRelease => PackageVersion.Contains("-dev");

	// Directories
	public string ProjectDirectory { get; }
	public string SourceDirectory => ProjectDirectory + "src/";
	public string OutputDirectory => ProjectDirectory + "bin/" + Configuration + "/";
	public string ZipDirectory => ProjectDirectory + "zip/";
	public string NuGetDirectory => ProjectDirectory + "nuget/";
	public string ChocoDirectory => ProjectDirectory + "choco/";
	public string PackageDirectory => ProjectDirectory + "package/";
	public string ZipImageDirectory => PackageDirectory + "zipimage/";
	public string PackageTestDirectory => PackageDirectory + "test/";
	public string ZipTestDirectory => PackageTestDirectory + "zip/";
	public string NuGetTestDirectory => PackageTestDirectory + "nuget/";
	public string ChocolateyTestDirectory => PackageTestDirectory + "choco/";

	// Checking 
	public string[] StandardHeader => DEFAULT_STANDARD_HEADER;
	public string[] ExemptFiles => new string[0];
	public bool CheckAssemblyInfoHeaders => false;

	// Packaging
	public string NuGetPackageName => $"{NUGET_ID}.{PackageVersion}.nupkg";
	public string NuGetPackage => PackageDirectory + NuGetPackageName;
	public string ChocolateyPackageName => $"{CHOCO_ID}.{PackageVersion}.nupkg";
	public string ChocolateyPackage => PackageDirectory + ChocolateyPackageName;
}