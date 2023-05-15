//////////////////////////////////////////////////////////////////////
// DUMP SETTINGS
//////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////
// BUILD SETTINGS
//////////////////////////////////////////////////////////////////////

public static class BuildSettings
{
	private static BuildSystem _buildSystem;

	static BuildSettings()
	{
		Tasks = new BuildTasks();
	}

	public static void Initialize(
	    // Required parameters
		ICakeContext context,
		string title,
		string githubRepository,

		// Optional parameters

		// If not specified, uses TITLE.sln if it exists or uses solution
		// found in the root directory provided there is only one. 
		string solutionFile = null,
        // Defaults to "**/*.tests.dll|**/*.tests.exe" (case insensitive)
		string unitTests = null,
		// Defaults to NUnitLite runner
		TestRunner unitTestRunner = null,
		string githubOwner = "TestCentric",
		// Defaults to our standard header
		string[] standardHeader = null,
		// Ignored if non-standard header is specified otherwise replaces line 2 of standard header
		string copyright = null,
		string[] exemptFiles = null, 
		bool msbuildAllowPreviewVersion = false,
		Verbosity msbuildVerbosity = Verbosity.Minimal,
		NuGetVerbosity nugetVerbosity = NuGetVerbosity.Normal,
		// Defaults to Debug and Release
		string[] validConfigurations = null,
		// If 0, is calculated based on branch name and package version
		int packageTestLevel = 0)
	{
		if (context == null)
			throw new ArgumentNullException(nameof(context));
		if (title == null)
			throw new ArgumentNullException(nameof(title));
		if (githubRepository == null)
			throw new ArgumentNullException(nameof(githubRepository));

		Context = context;
		_buildSystem = context.BuildSystem();


		Title = title;
		SolutionFile = solutionFile ?? DeduceSolutionFile();

		UnitTests = unitTests;
		UnitTestRunner = unitTestRunner;

		BuildVersion = new BuildVersion(context);

		PackageTestLevel = CalcPackageTestLevel(packageTestLevel);

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

		NuGetVerbosity = nugetVerbosity;

		ValidConfigurations = validConfigurations ?? DEFAULT_VALID_CONFIGS;
		Configuration = context.Argument("configuration", DEFAULT_CONFIGURATION);

		ValidateSettings();

		// Fix up dependencies that depend on the settings
		Tasks.FixupDependencies();

		context.Information($"{Title} {Configuration} version {PackageVersion}");

		// Output like this should go after the run title display
		if (solutionFile == null && SolutionFile != null)
			Context.Warning($"  SolutionFile: '{SolutionFile}'");
		Context.Information($"  PackageTestLevel: {PackageTestLevel}");

		if (IsRunningOnAppVeyor)
		{
			var buildNumber = _buildSystem.AppVeyor.Environment.Build.Number;
			_buildSystem.AppVeyor.UpdateBuildVersion($"{PackageVersion}-{buildNumber}");
		}
	}

	// Try to figure out solution file when not provided
	private static string DeduceSolutionFile()			
	{
		string solutionFile = null;

		if (System.IO.File.Exists(Title + ".sln"))
			solutionFile = Title + ".sln";
		else
		{
			var files = System.IO.Directory.GetFiles(ProjectDirectory, "*.sln");
			if (files.Count() == 1 && System.IO.File.Exists(files[0]))
				solutionFile = files[0];
		}

		return solutionFile;
	}

	private static int CalcPackageTestLevel(int initializeArgument)
	{
		// Command-line argument takes precedence
		int commandLineArgument = Context.Argument("testLevel", Context.Argument("level", 0));
		if (commandLineArgument > 0)
			return commandLineArgument;

		if (initializeArgument > 0)
			return initializeArgument;

		if (!BuildVersion.IsPreRelease)
			return 3;

		// TODO: The prerelease label is no longer being set to pr by GitVersion
		// for some reason. This check in AppVeyor is a workaround.
		if (IsRunningOnAppVeyor && _buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest)
			return 2;
		
		switch (BuildVersion.PreReleaseLabel)
		{
			case "pre":
			case "rc":
			case "alpha":
			case "beta":
				return 3;

			case "dev":
			case "pr":
				return 2;

			case "ci":
			default:
				return 1;
		}
	}

	// Cake Context
	public static ICakeContext Context { get; private set; }

	// Targets - not set until Setup runs
	public static string Target { get; set; }
	public static IEnumerable<string> TasksToExecute { get; set; }

	// Task Definitions
	public static BuildTasks Tasks { get; }
	
	// Arguments
	public static string Configuration { get; private set; }
	public static bool NoPush => Context.HasArgument("nopush");

	// Build Environment
	public static bool IsLocalBuild => _buildSystem.IsLocalBuild;
	public static bool IsRunningOnUnix => Context.IsRunningOnUnix();
	public static bool IsRunningOnWindows => Context.IsRunningOnWindows();
	public static bool IsRunningOnAppVeyor => _buildSystem.AppVeyor.IsRunningOnAppVeyor;

	// Versioning
	public static BuildVersion BuildVersion { get; private set; }
	public static string BranchName => BuildVersion.BranchName;
	public static bool IsReleaseBranch => BuildVersion.IsReleaseBranch;
	public static string PackageVersion => BuildVersion.PackageVersion;
	public static string AssemblyVersion => BuildVersion.AssemblyVersion;
	public static string AssemblyFileVersion => BuildVersion.AssemblyFileVersion;
	public static string AssemblyInformationalVersion => BuildVersion.AssemblyInformationalVersion;
	public static bool IsDevelopmentRelease => PackageVersion.Contains("-dev");

	// Standard Directory Structure - not changeable by user
	public static string ProjectDirectory => Context.Environment.WorkingDirectory.FullPath + "/";
	public static string SourceDirectory => ProjectDirectory + "src/";
	public static string OutputDirectory => ProjectDirectory + "bin/" + Configuration + "/";
	public static string ZipDirectory => ProjectDirectory + "zip/";
	public static string NuGetDirectory => ProjectDirectory + "nuget/";
	public static string ChocolateyDirectory => ProjectDirectory + "choco/";
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

	public static NuGetVerbosity NuGetVerbosity{ get; set; }
	public static NuGetRestoreSettings RestoreSettings => new NuGetRestoreSettings
	{
		Verbosity = NuGetVerbosity
	};
	public static NuGetInstallSettings NuGetInstallSettings => new NuGetInstallSettings
	{
		Verbosity = NuGetVerbosity
	};

	//Testing
	public static string UnitTests { get; set; }
	public static TestRunner UnitTestRunner {get; private set; }

	// Checking 
	public static string[] StandardHeader { get; private set; }
	public static string[] ExemptFiles { get; private set; }

	// Packaging
	public static string Title { get; private set; }
    public static List<PackageDefinition> Packages { get; } = new List<PackageDefinition>();

	// Package Testing
	public static int PackageTestLevel { get; set; }

	// Publishing - MyGet
	public static string MyGetPushUrl => MYGET_PUSH_URL;
	public static string MyGetApiKey => GetApiKey(TESTCENTRIC_MYGET_API_KEY, MYGET_API_KEY);

	// Publishing - NuGet
	public static string NuGetPushUrl => NUGET_PUSH_URL;
	public static string NuGetApiKey => GetApiKey(TESTCENTRIC_NUGET_API_KEY, NUGET_API_KEY);

	// Publishing - Chocolatey
	public static string ChocolateyPushUrl => CHOCO_PUSH_URL;
	public static string ChocolateyApiKey => GetApiKey(TESTCENTRIC_CHOCO_API_KEY, CHOCO_API_KEY);

	// Publishing - GitHub
	public static string GitHubOwner { get; set; }
	public static string GitHubRepository { get; set; }
	public static string GitHubAccessToken => GetApiKey(GITHUB_ACCESS_TOKEN);

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
		DisplayHeading("TASKS");
		DisplaySetting("Target:                       ", Target ?? "NOT SET");
		DisplaySetting("TasksToExecute:               ", TasksToExecute != null
			? string.Join(", ", TasksToExecute)
			: "NOT SET");

		DisplayHeading("ENVIRONMENT");
		DisplaySetting("IsLocalBuild:                 ", IsLocalBuild);
		DisplaySetting("IsRunningOnWindows:           ", IsRunningOnWindows);
		DisplaySetting("IsRunningOnUnix:              ", IsRunningOnUnix);
		DisplaySetting("IsRunningOnAppVeyor:          ", IsRunningOnAppVeyor);

		DisplayHeading("VERSIONING");
		DisplaySetting("PackageVersion:               ", PackageVersion);
		DisplaySetting("AssemblyVersion:              ", AssemblyVersion);
		DisplaySetting("AssemblyFileVersion:          ", AssemblyFileVersion);
		DisplaySetting("AssemblyInformationalVersion: ", AssemblyInformationalVersion);
		DisplaySetting("SemVer:                       ", BuildVersion.SemVer);
		DisplaySetting("IsPreRelease:                 ", BuildVersion.IsPreRelease);
		DisplaySetting("PreReleaseLabel:              ", BuildVersion.PreReleaseLabel);
		DisplaySetting("PreReleaseSuffix:             ", BuildVersion.PreReleaseSuffix);

		DisplayHeading("DIRECTORIES");
		DisplaySetting("Project:          ", ProjectDirectory);
		DisplaySetting("Output:           ", OutputDirectory);
		DisplaySetting("Source:           ", SourceDirectory);
		DisplaySetting("NuGet:            ", NuGetDirectory);
		DisplaySetting("Chocolatey:       ", ChocolateyDirectory);
		DisplaySetting("Package:          ", PackageDirectory);
		DisplaySetting("ZipImage:         ", ZipImageDirectory);
		DisplaySetting("ZipTest:          ", ZipTestDirectory);
		DisplaySetting("NuGetTest:        ", NuGetTestDirectory);
		DisplaySetting("ChocolateyTest:   ", ChocolateyTestDirectory);

		DisplayHeading("BUILD");
		DisplaySetting("Title:            ", Title);
		DisplaySetting("SolutionFile:     ", SolutionFile);
		DisplaySetting("Configuration:    ", Configuration);

		DisplayHeading("TESTING");
		DisplaySetting("UnitTests:        ", UnitTests, "DEFAULT");
		DisplaySetting("UnitTestRunner:   ", UnitTestRunner, "NUNITLITE");
		DisplaySetting("PackageTestLevel: ", PackageTestLevel);

		DisplayHeading("PACKAGES");
		if (Packages == null)
			Context.Error("NULL");
		else if (Packages.Count == 0)
			Context.Information("NONE");
		else
			foreach (PackageDefinition package in Packages)
			{
				DisplaySetting("", package?.PackageId);
				DisplaySetting($"  FileName: ", package?.PackageFileName);
				DisplaySetting($"  FilePath: ", package?.PackageFilePath);
			}

		DisplayHeading("PUBLISHING");
		DisplaySetting("ShouldPublishToMyGet:      ", ShouldPublishToMyGet);
		DisplaySetting("  MyGetPushUrl:            ", MyGetPushUrl);
		DisplaySetting("  MyGetApiKey:             ", KeyAvailable(TESTCENTRIC_MYGET_API_KEY, MYGET_API_KEY));
		DisplaySetting("ShouldPublishToNuGet:      ", ShouldPublishToNuGet);
		DisplaySetting("  NuGetPushUrl:            ", NuGetPushUrl);
		DisplaySetting("  NuGetApiKey:             ", KeyAvailable(TESTCENTRIC_NUGET_API_KEY, NUGET_API_KEY));
		DisplaySetting("ShouldPublishToChocolatey: ", ShouldPublishToNuGet);
		DisplaySetting("  ChocolateyPushUrl:       ", NuGetPushUrl);
		DisplaySetting("  ChocolateyApiKey:        ", KeyAvailable(TESTCENTRIC_CHOCO_API_KEY, CHOCO_API_KEY));
		DisplaySetting("NoPush:                    ", NoPush);

		DisplayHeading("\nRELEASING");
		DisplaySetting("BranchName:             ", BranchName);
		DisplaySetting("IsReleaseBranch:        ", IsReleaseBranch);
		DisplaySetting("IsProductionRelease:    ", IsProductionRelease);
		DisplaySetting("GitHubAccessToken:      ", KeyAvailable(GITHUB_ACCESS_TOKEN));
	}

	private static void DisplayHeading(string heading)
	{
		Context.Information($"\n{heading}");
	}

	private static void DisplaySetting<T>(string label, T setting, string notset="NOT SET")
	{
		var fmtSetting = setting == null ? notset : setting.ToString(); 
		Context.Information(label + fmtSetting);
	}

    private static string GetApiKey(string name, string fallback=null)
    {
        var apikey = Context.EnvironmentVariable(name);

        if (string.IsNullOrEmpty(apikey) && fallback != null)
            apikey = Context.EnvironmentVariable(fallback);

        return apikey;
    }

	private static string KeyAvailable(string name, string fallback=null)
	{
		return !string.IsNullOrEmpty(GetApiKey(name, fallback)) ? "AVAILABLE" : "NOT AVAILABLE";
	}
}