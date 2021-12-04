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
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };

// Defaults
const string DEFAULT_CONFIGURATION = "Release";
const string DEFAULT_GUI_VERSION = "2.0.0";

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
	private BuildSystem _buildSystem;

	public BuildParameters(ISetupContext context)
    {
        _context = context;
		_buildSystem = _context.BuildSystem();

		Target = _context.TargetTask.Name;
		TasksToExecute = _context.TasksToExecute.Select(t => t.Name);

		Configuration = _context.Argument("configuration", DEFAULT_CONFIGURATION);
		ProjectDirectory = _context.Environment.WorkingDirectory.FullPath + "/";

		MyGetApiKey = _context.EnvironmentVariable(MYGET_API_KEY);
		NuGetApiKey = _context.EnvironmentVariable(NUGET_API_KEY);
		ChocolateyApiKey = _context.EnvironmentVariable(CHOCO_API_KEY);
		GitHubAccessToken = _context.EnvironmentVariable(GITHUB_ACCESS_TOKEN);

		BuildVersion = new BuildVersion(context);
    }

	// Targets
	public string Target { get; }
	public IEnumerable<string> TasksToExecute { get; }

	// Setup Context
	public ISetupContext Context => _context;

	// Arguments
	public string Configuration { get; }

	// Versioning
	public BuildVersion BuildVersion { get; }
	public string BranchName => BuildVersion.BranchName;
	public bool IsReleaseBranch => BuildVersion.IsReleaseBranch;
	public string PackageVersion => BuildVersion.PackageVersion;
	public string AssemblyVersion => BuildVersion.AssemblyVersion;
	public string AssemblyFileVersion => BuildVersion.AssemblyFileVersion;
	public string AssemblyInformationalVersion => BuildVersion.AssemblyInformationalVersion;
	public bool IsDevelopmentRelease => PackageVersion.Contains("-dev");

	public bool IsLocalBuild => _buildSystem.IsLocalBuild;
	public bool IsRunningOnUnix => _context.IsRunningOnUnix();
	public bool IsRunningOnWindows => _context.IsRunningOnWindows();
	public bool IsRunningOnAppVeyor => _buildSystem.AppVeyor.IsRunningOnAppVeyor;

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
	public string NuGetId { get; set; }
	public string ChocoId { get; set; }
	public string NuGetPackageName => $"{NuGetId}.{PackageVersion}.nupkg";
	public string NuGetPackage => PackageDirectory + NuGetPackageName;
	public string ChocolateyPackageName => $"{ChocoId}.{PackageVersion}.nupkg";
	public string ChocolateyPackage => PackageDirectory + ChocolateyPackageName;

	// Package Testing
	public string GuiVersion { get; set; } = DEFAULT_GUI_VERSION;

	// Publishing
	public string MyGetPushUrl => MYGET_PUSH_URL;
	public string NuGetPushUrl => NUGET_PUSH_URL;
	public string ChocolateyPushUrl => CHOCO_PUSH_URL;

	public string MyGetApiKey { get; }
	public string NuGetApiKey { get; }
	public string ChocolateyApiKey { get; }
	public string GitHubAccessToken { get; }

	//public bool ShouldPublishToMyGet => IsDevelopmentRelease;
	public bool IsPreRelease => BuildVersion.IsPreRelease;
	public bool ShouldPublishToMyGet =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_MYGET.Contains(BuildVersion.PreReleaseLabel);
	public bool ShouldPublishToNuGet =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_NUGET.Contains(BuildVersion.PreReleaseLabel);
	public bool ShouldPublishToChocolatey =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_CHOCOLATEY.Contains(BuildVersion.PreReleaseLabel);
	public bool IsProductionRelease =>
		!IsPreRelease || LABELS_WE_RELEASE_ON_GITHUB.Contains(BuildVersion.PreReleaseLabel);
}