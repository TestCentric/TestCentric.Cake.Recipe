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
		string githubRepository,
     
	 // Optional parameters
		bool suppressHeaderCheck = false,
		string[] standardHeader = null,
		string copyright = null,
		string[] exemptFiles = null,

		string solutionFile = null,
		string[] validConfigurations = null,
		bool msbuildAllowPreviewVersion = false,
		string githubOwner = "TestCentric",
		
		string unitTests = null, // Defaults to "**/*.tests.dll|**/*.tests.exe" (case insensitive)
		TestRunner unitTestRunner = null, // If not set, NUnitLite is used
		string unitTestArguments = null,

		string defaultTarget = null, // Defaults to "Build"
		
		// Verbosity
		Verbosity msbuildVerbosity = Verbosity.Minimal,
		NuGetVerbosity nugetVerbosity = NuGetVerbosity.Normal,
		bool chocolateyVerbosity = false )
	{
		// Required arguments
		if (context == null)
			throw new ArgumentNullException(nameof(context));
		if (title == null)
			throw new ArgumentNullException(nameof(title));
		if (githubRepository == null)
			throw new ArgumentNullException(nameof(githubRepository));

		Context = context;
		Title = title;
		GitHubRepository = githubRepository;

		// NOTE: Order of initialization can be sensitive. Obviously,
		// we have to set any properties in this method before we
		// make use of them. Less obviously, some of the classes we
		// construct here have dependencies on certain properties
		// being set before the constructor is called. I have
		// tried to annotate such dependenciess below.

		_buildSystem = context.BuildSystem();

		// If not specified, uses TITLE.sln if it exists or uses solution
		// found in the root directory provided there is only one. 
		SolutionFile = solutionFile ?? DeduceSolutionFile();

		ValidConfigurations = validConfigurations ?? DEFAULT_VALID_CONFIGS;

		UnitTests = unitTests;
		// NUnitLiteRunner depends indirectly on ValidConfigurations
		UnitTestRunner = unitTestRunner ?? new NUnitLiteRunner();
		UnitTestArguments = unitTestArguments;

		BuildVersion = new BuildVersion(context);

		GitHubOwner = githubOwner;

		// File Header Checks
		SuppressHeaderCheck = suppressHeaderCheck && !CommandLineOptions.NoBuild;
		StandardHeader = standardHeader;
		if (standardHeader == null)
		{
			StandardHeader = DEFAULT_STANDARD_HEADER;
			// We can only replace copyright line in the default header
			if (copyright != null)
				StandardHeader[1] = "// " + copyright;
		}
		ExemptFiles = exemptFiles ?? new string[0];

		if (defaultTarget != null)
			BuildTasks.DefaultTask.IsDependentOn(defaultTarget);

		MSBuildVerbosity = msbuildVerbosity;
		MSBuildAllowPreviewVersion = msbuildAllowPreviewVersion;

		NuGetVerbosity = nugetVerbosity;
		ChocolateyVerbosity = chocolateyVerbosity;

		// Skip remaining initialization if help was requested
		if (CommandLineOptions.Usage)
			return;
			
		ValidateSettings();

		context.Information($"{Title} {Configuration} version {PackageVersion}");

		// Output like this should go after the run title display
		if (solutionFile == null && SolutionFile != null)
			Context.Warning($"  SolutionFile: '{SolutionFile}'");
		Context.Information($"  PackageTestLevel: {PackageTestLevel}");

		// Keep this last
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

	private static int CalcPackageTestLevel()
	{
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

	// Arguments
	public static string Configuration
	{
		get
		{
			// Correct casing on user-provided config if necessary
			foreach (string config in ValidConfigurations)
				if (string.Equals(config, CommandLineOptions.Configuration.Value, StringComparison.OrdinalIgnoreCase))
					return config;

			// Return the (invalid) user-provided config
			return CommandLineOptions.Configuration.Value;
		}
	}

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
	public static string SourceDirectory				=> ProjectDirectory + SRC_DIR;
	public static string OutputDirectory				=> ProjectDirectory + BIN_DIR + Configuration + "/";
	public static string ZipDirectory					=> ProjectDirectory + ZIP_DIR;
	public static string NuGetDirectory					=> ProjectDirectory + NUGET_DIR;
	public static string ChocolateyDirectory			=> ProjectDirectory + CHOCO_DIR;
	public static string PackagingDirectory				=> ProjectDirectory + PACKAGING_DIR;
	public static string ZipImageDirectory				=> ProjectDirectory + ZIP_IMG_DIR;
	public static string ToolsDirectory					=> ProjectDirectory + TOOLS_DIR;
	public static string PackageTestDirectory			=> ProjectDirectory + PKG_TEST_DIR;
	public static string ZipTestDirectory				=> ProjectDirectory + ZIP_TEST_DIR;
	public static string NuGetTestDirectory				=> ProjectDirectory + NUGET_TEST_DIR;
	public static string NuGetTestRunnerDirectory		=> ProjectDirectory + NUGET_RUNNER_DIR;
	public static string ChocolateyTestDirectory		=> ProjectDirectory + CHOCO_TEST_DIR;
	public static string ChocolateyTestRunnerDirectory	=> ProjectDirectory + CHOCO_RUNNER_DIR;
	public static string PackageResultDirectory			=> ProjectDirectory + PKG_RSLT_DIR;
	public static string ZipResultDirectory				=> ProjectDirectory + ZIP_RSLT_DIR;
	public static string NuGetResultDirectory			=> ProjectDirectory + NUGET_RSLT_DIR;
	public static string ChocolateyResultDirectory		=> ProjectDirectory + CHOCO_RSLT_DIR;
	public static string LocalPackagesDirectory			=> ProjectDirectory + LOCAL_PACKAGES_DIR;

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

	public static bool ShouldAddToLocalFeed =>
		IsLocalBuild
			? !IsPreRelease || LABELS_WE_ADD_TO_LOCAL_FEED.Contains(BuildVersion.PreReleaseLabel)
			: false;

	public static NuGetVerbosity NuGetVerbosity{ get; set; }
	public static NuGetRestoreSettings RestoreSettings => new NuGetRestoreSettings
	{
		Verbosity = NuGetVerbosity
	};

	// The chocolatey Setting is actually bool Verbose, but we use verbosity 
	// so it lines up with the settings for NuGet
	public static bool ChocolateyVerbosity { get; set; }

	//Testing
	public static string UnitTests { get; set; }
	public static TestRunner UnitTestRunner { get; private set; }
	public static string UnitTestArguments { get; private set; }

	// Checking 
	public static bool SuppressHeaderCheck { get; private set; }
	public static string[] StandardHeader { get; private set; }
	public static string[] ExemptFiles { get; private set; }

	// Packaging
	public static string Title { get; private set; }
    public static List<PackageDefinition> Packages { get; } = new List<PackageDefinition>();

	// Package Testing
	public static int PackageTestLevel =>
		CommandLineOptions.TestLevel.Value > 0
			? CommandLineOptions.TestLevel.Value
			: CalcPackageTestLevel();

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
		
		if (!ValidConfigurations.Contains(Configuration))
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

		DisplayHeading("COMMAND-LINE OPTIONS");
		DisplaySetting("Target:           ", CommandLineOptions.Target.Value);
		DisplaySetting("Configuration:    ", CommandLineOptions.Configuration.Value);
		DisplaySetting("PackageVersion:   ", CommandLineOptions.PackageVersion.Value);
		DisplaySetting("TestLevel:        ", CommandLineOptions.TestLevel.Value);
		DisplaySetting("TraceLevel:       ", CommandLineOptions.TraceLevel.Value);
		DisplaySetting("NoBuild:          ", CommandLineOptions.NoBuild ? "True" : "NOT SET");
		DisplaySetting("NoPush:           ", CommandLineOptions.NoPush ? "True" : "NOT SET");

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
		DisplaySetting("Package:          ", PackagingDirectory);
		DisplaySetting("ZipImage:         ", ZipImageDirectory);
		DisplaySetting("ZipTest:          ", ZipTestDirectory);
		DisplaySetting("NuGetTest:        ", NuGetTestDirectory);
		DisplaySetting("ChocolateyTest:   ", ChocolateyTestDirectory);
		DisplaySetting("LocalPackages:    ", LocalPackagesDirectory);

		DisplayHeading("BUILD");
		DisplaySetting("Title:            ", Title);
		DisplaySetting("SolutionFile:     ", SolutionFile);

		DisplayHeading("TESTING");
		DisplaySetting("UnitTests:        ", UnitTests, "DEFAULT");
		DisplaySetting("UnitTestRunner:   ", UnitTestRunner, "NUNITLITE");

		DisplayHeading("PACKAGES");
		if (Packages == null)
			Context.Error("NULL");
		else if (Packages.Count == 0)
			Context.Information("NONE");
		else
			foreach (PackageDefinition package in Packages)
			{
				DisplaySetting("", package?.PackageId);
				DisplaySetting($"  PackageSource:  ", package?.PackageSource);
				DisplaySetting($"  FileName:       ", package?.PackageFileName);
				DisplaySetting($"  FilePath:       ", package?.PackageFilePath);
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
