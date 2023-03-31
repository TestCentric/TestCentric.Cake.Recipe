//////////////////////////////////////////////////////////////////////
// DUMP SETTINGS
//////////////////////////////////////////////////////////////////////

Task("DumpSettings")
	.Does(() => BuildSettings.DumpSettings());

//////////////////////////////////////////////////////////////////////
// BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

public static class BuildSettings
{
	private static BuildSystem _buildSystem;

	public static void Initialize(
		// Required parameters
		ICakeContext context,
		string title,
		// Optional named parameters
		string solutionFile = null,
        string unitTests = null,
		string guiVersion = null,
		string githubOwner = "TestCentric",
		string githubRepository = null,
		string copyright = null,
		string[] standardHeader = null,
		string[] exemptFiles = null,
		bool msbuildAllowPreviewVersion = false,
		Verbosity msbuildVerbosity = Verbosity.Minimal,
		string[] validConfigurations = null,
		int packageTestLevel = 0)
	{
		if (context == null)
			throw new ArgumentNullException(nameof(context));
		if (title == null)
			throw new ArgumentNullException(nameof(title));

		Context = context;
		_buildSystem = context.BuildSystem();

		Title = title;
		SolutionFile = solutionFile;
		if (solutionFile == null && title != null)
		{
			var sln = title + ".sln";
			if (System.IO.File.Exists(sln))
				SolutionFile = sln;
		}
		UnitTests = unitTests;

		BuildVersion = new BuildVersion(context);

		// Command-line argument
		PackageTestLevel = context.Argument("testLevel", context.Argument("level", 0));
		if (PackageTestLevel == 0) // Argument supplied in build.cake
			PackageTestLevel = packageTestLevel;
		if (PackageTestLevel == 0) // Use defaults
			if (!BuildVersion.IsPreRelease)
				PackageTestLevel = 3;
			else switch (BuildVersion.PreReleaseLabel)
			{
				case "pre":
				case "rc":
				case "alpha":
				case "beta":
					PackageTestLevel = 3;
					break;
				case "dev":
				case "pr":
					PackageTestLevel = 2;
					break;
				case "ci":
				default:
					PackageTestLevel = 1;
					break;
			}

		GitHubOwner = githubOwner;
		GitHubRepository = githubRepository;

		StandardHeader = standardHeader;
		if (standardHeader == null)
		{
			StandardHeader = DEFAULT_STANDARD_HEADER;
			// We can only replace copyright line in the default header
			if (copyright != null)
				StandardHeader[1] = "// " + copyright;
		}
		ExemptFiles = exemptFiles ?? new string[0];

		MSBuildVerbosity = msbuildVerbosity;
		MSBuildAllowPreviewVersion = msbuildAllowPreviewVersion;

		ValidConfigurations = validConfigurations ?? DEFAULT_VALID_CONFIGS;
		Configuration = context.Argument("configuration", DEFAULT_CONFIGURATION);

		ValidateSettings();
	}

	// Cake Context
	public static ICakeContext Context { get; private set; }

	// Targets - not set until Setup runs
	public static string Target { get; set; }
	public static IEnumerable<string> TasksToExecute { get; set; }
	
	// Arguments
	public static string Configuration { get; private set; }
	public static bool NoPush => Context.HasArgument("nopush");

	// Versioning
	public static BuildVersion BuildVersion { get; private set; }
	public static string BranchName => BuildVersion.BranchName;
	public static bool IsReleaseBranch => BuildVersion.IsReleaseBranch;
	public static string PackageVersion => BuildVersion.PackageVersion;
	public static string AssemblyVersion => BuildVersion.AssemblyVersion;
	public static string AssemblyFileVersion => BuildVersion.AssemblyFileVersion;
	public static string AssemblyInformationalVersion => BuildVersion.AssemblyInformationalVersion;
	public static bool IsDevelopmentRelease => PackageVersion.Contains("-dev");

	// Build System
	public static bool IsLocalBuild => _buildSystem.IsLocalBuild;
	public static bool IsRunningOnUnix => Context.IsRunningOnUnix();
	public static bool IsRunningOnWindows => Context.IsRunningOnWindows();
	public static bool IsRunningOnAppVeyor => _buildSystem.AppVeyor.IsRunningOnAppVeyor;

	// Standard Directory Structure - not changeable by user
	public static string ProjectDirectory => Context.Environment.WorkingDirectory.FullPath + "/";
	public static string SourceDirectory => ProjectDirectory + "src/";
	public static string OutputDirectory => ProjectDirectory + "bin/" + Configuration + "/";
	public static string ZipDirectory => ProjectDirectory + "zip/";
	public static string NuGetDirectory => ProjectDirectory + "nuget/";
	public static string ChocoDirectory => ProjectDirectory + "choco/";
	public static string PackageDirectory => ProjectDirectory + "package/";
	public static string ZipImageDirectory => PackageDirectory + "zipimage/";
	public static string ToolsDirectory => ProjectDirectory + "tools/";
	public static string PackageTestDirectory => PackageDirectory + "tests/";
	public static string ZipTestDirectory => PackageTestDirectory + "zip/";
	public static string NuGetTestDirectory => PackageTestDirectory + "nuget/";
	public static string NuGetTestRunnerDirectory => NuGetTestDirectory + "runners/";
	public static string ChocolateyTestDirectory => PackageTestDirectory + "choco/";
	public static string ChocolateyTestRunnerDirectory => ChocolateyTestDirectory + "runners/";
	public static string PackageResultDirectory => PackageDirectory + "results/";
	public static string ZipResultDirectory => PackageResultDirectory + "zip/";
	public static string NuGetResultDirectory => PackageResultDirectory + "nuget/";
	public static string ChocolateyResultDirectory => PackageResultDirectory + "choco/";

	// Files
	public static string SolutionFile { get; set; }

	// Building
	public static string[] ValidConfigurations { get; set; }
	public static bool MSBuildAllowPreviewVersion { get; set; }
	public static Verbosity MSBuildVerbosity { get; set; }
	public static MSBuildSettings MSBuildSettings => new MSBuildSettings {
		Verbosity = MSBuildVerbosity,
		Configuration = Configuration,
		PlatformTarget = PlatformTarget.MSIL,
		AllowPreviewVersion = MSBuildAllowPreviewVersion
	};
	public static NuGetRestoreSettings RestoreSettings => new NuGetRestoreSettings();

	//Testing
	public static string UnitTests { get; set; }

	// Checking 
	public static string[] StandardHeader { get; private set; }
	public static string[] ExemptFiles { get; private set; }

	// Packaging
	public static string Title { get; private set; }
    public static List<PackageDefinition> Packages { get; } = new List<PackageDefinition>();

	// Package Testing
	public static int PackageTestLevel { get; set; }

    public static ExtensionSpecifier NUnitV2Driver => new ExtensionSpecifier("NUnit.Extension.NUnitV2Driver", "nunit-extension-nunit-v2-driver", "3.9.0");
    public static ExtensionSpecifier NUnitProjectLoader => new ExtensionSpecifier("NUnit.Extension.NUnitProjectLoader", "nunit-extension-nunit-project-loader", "3.7.1");
    public static ExtensionSpecifier Net20PluggableAgent => new ExtensionSpecifier("NUnit.Extension.Net20PluggableAgent", "nunit-extension-net20-pluggable-agent", "2.0.0");
    public static ExtensionSpecifier NetCore21PluggableAgent => new ExtensionSpecifier("NUnit.Extension.NetCore21PluggableAgent", "nunit-extension-netcore21-pluggable-agent", "2.1.0");
    public static ExtensionSpecifier Net80PluggableAgent => new ExtensionSpecifier("NUnit.Extension.Net80PluggableAgent", "nunit-extension-net80-pluggable-agent", "2.1.0");

	// Publishing - MyGet
	public static string MyGetPushUrl => MYGET_PUSH_URL;
	public static string MyGetApiKey => Context.EnvironmentVariable(MYGET_API_KEY);

	// Publishing - NuGet
	public static string NuGetPushUrl => NUGET_PUSH_URL;
	public static string NuGetApiKey => Context.EnvironmentVariable(NUGET_API_KEY);

	// Publishing - Chocolatey
	public static string ChocolateyPushUrl => CHOCO_PUSH_URL;
	public static string ChocolateyApiKey => Context.EnvironmentVariable(CHOCO_API_KEY);

	// Publishing - GitHub
	public static string GitHubOwner { get; set; }
	public static string GitHubRepository { get; set; }
	public static string GitHubAccessToken => Context.EnvironmentVariable(GITHUB_ACCESS_TOKEN);

	public static bool IsPreRelease => BuildVersion.IsPreRelease;
	public static bool ShouldPublishToMyGet =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_MYGET.Contains(BuildVersion.PreReleaseLabel);
	public static bool ShouldPublishToNuGet =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_NUGET.Contains(BuildVersion.PreReleaseLabel);
	public static bool ShouldPublishToChocolatey =>
		!IsPreRelease || LABELS_WE_PUBLISH_ON_CHOCOLATEY.Contains(BuildVersion.PreReleaseLabel);
	public static bool IsProductionRelease =>
		!IsPreRelease || LABELS_WE_RELEASE_ON_GITHUB.Contains(BuildVersion.PreReleaseLabel);

	private static void ValidateSettings()
	{
		var validationErrors = new List<string>();
		
		bool validConfig = false;
		foreach (string config in ValidConfigurations)
		{
			if (string.Equals(config, Configuration, StringComparison.OrdinalIgnoreCase))
			{
				// Set again in case user specified wrong casing
				Configuration = config;
				validConfig = true;
			}
		}

		if (!validConfig)
			validationErrors.Add($"Invalid configuration: {Configuration}");

		if (validationErrors.Count > 0)
		{
			DumpSettings();

			var msg = new StringBuilder("Parameter validation failed! See settings above.\r\n\nErrors found:\r\n");
			foreach (var error in validationErrors)
				msg.AppendLine("  " + error);

			throw new InvalidOperationException(msg.ToString());
		}
	}

	public static void DumpSettings()
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
		Console.WriteLine("Title:           " + Title);
		Console.WriteLine("SolutionFile:    " + SolutionFile ?? "NULL");
		Console.WriteLine("Configuration:   " + Configuration);

		Console.WriteLine("\nTESTING");
		Console.WriteLine("UnitTests:       " + UnitTests ?? "DEFAULT");


		Console.WriteLine("\nPACKAGES");
		if (Packages == null)
			Console.WriteLine("NULL");
		else if (Packages.Count == 0)
			Console.WriteLine("NONE");
		else
			foreach (PackageDefinition package in Packages)
			{
				Console.WriteLine($"{package.PackageId}");
				Console.WriteLine($"  FileName: {package.PackageFileName}");
				Console.WriteLine($"  FilePath: {package.PackageFilePath}");
			}

		Console.WriteLine("\nPUBLISHING");
		Console.WriteLine("ShouldPublishToMyGet:   " + ShouldPublishToMyGet);
		Console.WriteLine("  MyGetPushUrl:         " + MyGetPushUrl);
		Console.WriteLine("  MyGetApiKey:          " + KeyAvailable(MYGET_API_KEY));
		Console.WriteLine("ShouldPublishToNuGet:   " + ShouldPublishToNuGet);
		Console.WriteLine("  NuGetPushUrl:         " + NuGetPushUrl);
		Console.WriteLine("  NuGetApiKey:          " + KeyAvailable(NUGET_API_KEY));
		Console.WriteLine("NoPush:                 " + NoPush);

		Console.WriteLine("\nRELEASING");
		Console.WriteLine("BranchName:             " + BranchName);
		Console.WriteLine("IsReleaseBranch:        " + IsReleaseBranch);
		Console.WriteLine("IsProductionRelease:    " + IsProductionRelease);
		Console.WriteLine("GitHubAccessToken:      " + KeyAvailable(GITHUB_ACCESS_TOKEN));
	}

    private static string GetApiKey(string name, string fallback=null)
    {
        var apikey = Context.EnvironmentVariable(name);

        if (string.IsNullOrEmpty(apikey) && fallback != null)
            apikey = Context.EnvironmentVariable(fallback);

        return apikey;
    }

	private static string KeyAvailable(string name)
	{
		return !string.IsNullOrEmpty(GetApiKey(name)) ? "AVAILABLE" : "NOT AVAILABLE";
	}
}
