/// <summary>
/// The TestRunner class is the abstract base for all TestRunners used to test packages.
/// A TestRunner knows how to run a test assembly and provide a result.
/// </summary>
public abstract class TestRunner
{
	public virtual bool RequiresInstallation => false;

	protected string ExecutablePath { get; set; }

	protected ProcessSettings ProcessSettings { get; } = new ProcessSettings()
	{
		WorkingDirectory = BuildSettings.OutputDirectory
	};

	public virtual int Run(string arguments=null)
	{
		if (ExecutablePath == null)
			throw new InvalidOperationException("Unable to run tests. Executable path has not been set.");

		if (ExecutablePath.EndsWith(".dll"))
		{
			ProcessSettings.Arguments = $"{ExecutablePath} {arguments}";
			return BuildSettings.Context.StartProcess("dotnet", ProcessSettings);
		}
		else
		{
			ProcessSettings.Arguments = arguments;
			return BuildSettings.Context.StartProcess(ExecutablePath, ProcessSettings);
		}
	}

	// Base install does nothing
	public virtual void Install() { } 
}

/// <summary>
/// The InstallableTestRunner class is the abstract base for TestRunners which
/// must be installed using a published package before they can be used.
/// </summary>
public abstract class InstallableTestRunner : TestRunner
{
	public override bool RequiresInstallation => true;

	public InstallableTestRunner(string packageId, string version)
	{
		if (packageId == null)
			throw new ArgumentNullException(nameof(packageId));
		if (version == null)
			throw new ArgumentNullException(nameof(version));

		PackageId = packageId;
		Version = version;
	}

	public string PackageId { get; }
	public string Version { get; }

	public abstract string InstallPath { get; }
}

public class NUnitLiteRunner : TestRunner
{
    public NUnitLiteRunner(string testPath)
    {
        ExecutablePath = testPath;
    }

    public override int Run(string arguments=null)
    {
        var traceLevel = CommandLineOptions.TraceLevel ?? "Off";

        ProcessSettings.EnvironmentVariables = new Dictionary<string,string> {
            { "TESTCENTRIC_INTERNAL_TRACE_LEVEL", traceLevel }
        };
        
        return base.Run(arguments);
    }
}

/// <summary>
/// Class that knows how to run an agent directly.
/// </summary>
public class AgentRunner : TestRunner
{
	public AgentRunner(string agentExecutable)
	{
		ExecutablePath = agentExecutable;
	}
}

/// <summary>
/// Class that knows how to install and run the TestCentric GUI,
/// using either the NuGet or the Chocolatey package.
/// </summary>
public class GuiRunner : InstallableTestRunner
{
	public const string NuGetId = "TestCentric.GuiRunner";
	public const string ChocoId = "testcentric-gui";

	private const string RUNNER_EXE = "testcentric.exe";

	public GuiRunner(string packageId, string version)
		: base(packageId, version)
	{
		if (packageId != NuGetId && packageId != ChocoId)
			throw new ArgumentException($"Package Id invalid: {packageId}", nameof(packageId));

		ExecutablePath = $"{InstallPath}{PackageId}.{Version}/tools/{RUNNER_EXE}";
	}

	public string BuiltInAgentUnderTest { get; set; }

	public override string InstallPath => PackageId == ChocoId
		? BuildSettings.ChocolateyTestRunnerDirectory
		: BuildSettings.NuGetTestRunnerDirectory;

	public override int Run(string arguments)
	{
		if (string.IsNullOrEmpty(arguments))
			throw new ArgumentException("No run arguments supplied");

		if (!arguments.Contains(" --run"))
			arguments += " --run";
		if (!arguments.Contains(" --unattended"))
			arguments += " --unattended";

		return base.Run(arguments);
	}

	public override void Install()
	{
		var packageSources = new []
		{
			"https://www.myget.org/F/testcentric/api/v3/index.json",
			PackageId == ChocoId
				? "https://community.chocolatey.org/api/v2/"
				: "https://api.nuget.org/v3/index.json"
		};

		// Use NuGet for installation even if using the Chocolatey 
		// package in order to avoid running as administrator.
		BuildSettings.Context.NuGetInstall(
			PackageId, 
			new NuGetInstallSettings()
			{
				Version = Version,
				OutputDirectory = InstallPath,
				Source = packageSources
			});

		// If we are testing one of the built-in agents, remove the copy of the agent
		// which was installed alongside the GUI so our new build is used.
		if (BuiltInAgentUnderTest != null)
			foreach (DirectoryPath directoryPath in BuildSettings.Context.GetDirectories($"{InstallPath}{BuiltInAgentUnderTest}*"))
				BuildSettings.Context.DeleteDirectory(
					directoryPath,
					new DeleteDirectorySettings() { Recursive = true });
	}
}
