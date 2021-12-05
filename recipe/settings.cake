#load "./check-headers.cake"
#load "./package-checks.cake"
#load "./test-results.cake"
#load "./test-report.cake"
#load "./package-tests.cake"
#load "./testcentric-gui.cake"
#load "./versioning.cake"
#load "./clean-targets.cake"
#load "./github-targets.cake"

Task("DisplaySettings")
	.Does<BuildSettings>((settings) =>
	{
		settings.DisplaySettings();
	});

public class BuildSettings
{
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
	const string DEFAULT_COPYRIGHT = "Copyright (c) Charlie Poole and TestCentric contributors.";
	static readonly string[] DEFAULT_STANDARD_HEADER = new[] {
		"// ***********************************************************************",
		$"// {DEFAULT_COPYRIGHT}",
		"// Licensed under the MIT License. See LICENSE.txt in root directory.",
		"// ***********************************************************************"
	};

	private ISetupContext _context;
	private BuildSystem _buildSystem;

	public static BuildSettings Initialize(
		ISetupContext context,
		string title = null,
		string nugetId = null,
		string chocoId = null,
		string guiVersion = null,
		string githubOwner = null,
		string githubRepository = null,
		string copyright = null,
		string[] standardHeader = null)
	{
		if (context == null)
			throw new ArgumentNullException("context");

		var parms = new BuildSettings(context);

		parms.Title = title;
		parms.NuGetId = nugetId;
		parms.ChocoId = chocoId;
		parms.GuiVersion = guiVersion ?? DEFAULT_GUI_VERSION;
		parms.GitHubOwner = githubOwner;
		parms.GitHubRepository = githubRepository;
		parms.StandardHeader = standardHeader;

		if (standardHeader == null)
		{
			parms.StandardHeader = DEFAULT_STANDARD_HEADER;
			// We can only replace copyright line in the default header
			if (copyright != null)
				parms.StandardHeader[1] = "// " + copyright;
		}

		return parms;
	}

	private BuildSettings(ISetupContext context)
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

	// Standard Directory Structure - not changeable by user
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
	public string[] StandardHeader { get; set; }
	public string[] ExemptFiles => new string[0];
	public bool CheckAssemblyInfoHeaders => false;

	// Packaging
	public string Title { get; set; }
	public string NuGetId { get; set; }
	public string ChocoId { get; set; }
	public string NuGetPackageName => $"{NuGetId}.{PackageVersion}.nupkg";
	public string NuGetPackage => PackageDirectory + NuGetPackageName;
	public string ChocolateyPackageName => $"{ChocoId}.{PackageVersion}.nupkg";
	public string ChocolateyPackage => PackageDirectory + ChocolateyPackageName;

	// Package Testing
	public string GuiVersion { get; set; } = DEFAULT_GUI_VERSION;

	// Publishing - MyGet
	public string MyGetPushUrl => MYGET_PUSH_URL;
	public string MyGetApiKey { get; }

	// Publishing - NuGet
	public string NuGetPushUrl => NUGET_PUSH_URL;
	public string NuGetApiKey { get; }

	// Publishing - Chocolatey
	public string ChocolateyPushUrl => CHOCO_PUSH_URL;
	public string ChocolateyApiKey { get; }

	// Publishing - GitHub
	public string GitHubOwner { get; set; }
	public string GitHubRepository { get; set; }
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

	public void DisplaySettings()
    {
		Console.WriteLine("\nTASKS");
		Console.WriteLine("Target:                       " + Target);
		Console.WriteLine("TasksToExecute:               " + string.Join(", ", TasksToExecute));

		Console.WriteLine("\nENVIRONMENT");
		Console.WriteLine("IsLocalBuild:                 " + IsLocalBuild);
		Console.WriteLine("IsRunningOnWindows:           " + IsRunningOnWindows);
		Console.WriteLine("IsRunningOnUnix:              " + IsRunningOnUnix);
		Console.WriteLine("IsRunningOnAppVeyor:          " + IsRunningOnAppVeyor);

		Console.WriteLine("\nVERSIONING");
		Console.WriteLine("PackageVersion:               " + PackageVersion);
		Console.WriteLine("AssemblyVersion:              " + AssemblyVersion);
		Console.WriteLine("AssemblyFileVersion:          " + AssemblyFileVersion);
		Console.WriteLine("AssemblyInformationalVersion: " + AssemblyInformationalVersion);
		Console.WriteLine("SemVer:                       " + BuildVersion.SemVer);
		Console.WriteLine("IsPreRelease:                 " + BuildVersion.IsPreRelease);
		Console.WriteLine("PreReleaseLabel:              " + BuildVersion.PreReleaseLabel);
		Console.WriteLine("PreReleaseSuffix:             " + BuildVersion.PreReleaseSuffix);

		Console.WriteLine("\nDIRECTORIES");
		Console.WriteLine("Project:   " + ProjectDirectory);
		Console.WriteLine("Output:    " + OutputDirectory);
		Console.WriteLine("Source:    " + SourceDirectory);
		Console.WriteLine("NuGet:     " + NuGetDirectory);
		Console.WriteLine("Choco:     " + ChocoDirectory);
		Console.WriteLine("Package:   " + PackageDirectory);
		Console.WriteLine("ZipImage:  " + ZipImageDirectory);
		Console.WriteLine("ZipTest:   " + ZipTestDirectory);
		Console.WriteLine("NuGetTest: " + NuGetTestDirectory);
		Console.WriteLine("ChocoTest: " + ChocolateyTestDirectory);

		Console.WriteLine("\nBUILD");
		Console.WriteLine("Configuration:   " + Configuration);
		//Console.WriteLine("Engine Runtimes: " + string.Join(", ", SupportedEngineRuntimes));

		Console.WriteLine("\nPACKAGING");
		Console.WriteLine("MyGetPushUrl:              " + MyGetPushUrl);
		Console.WriteLine("NuGetPushUrl:              " + NuGetPushUrl);
		Console.WriteLine("ChocolateyPushUrl:         " + ChocolateyPushUrl);
		Console.WriteLine("MyGetApiKey:               " + (!string.IsNullOrEmpty(MyGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("NuGetApiKey:               " + (!string.IsNullOrEmpty(NuGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("ChocolateyApiKey:          " + (!string.IsNullOrEmpty(ChocolateyApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));

		Console.WriteLine("\nPUBLISHING");
		Console.WriteLine("ShouldPublishToMyGet:      " + ShouldPublishToMyGet);
		Console.WriteLine("ShouldPublishToNuGet:      " + ShouldPublishToNuGet);
		Console.WriteLine("ShouldPublishToChocolatey: " + ShouldPublishToChocolatey);

		Console.WriteLine("\nRELEASING");
		Console.WriteLine("BranchName:                   " + BranchName);
		Console.WriteLine("IsReleaseBranch:              " + IsReleaseBranch);
		Console.WriteLine("IsProductionRelease:          " + IsProductionRelease);
	}
}