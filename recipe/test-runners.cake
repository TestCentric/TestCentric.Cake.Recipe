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

	public virtual int Run(FilePath executablePath, string arguments=null)
	{
		ExecutablePath = executablePath.ToString();
		return this.Run(arguments);
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
	public override int Run(FilePath testPath, string arguments=null)
	{
		Console.WriteLine($"NUnitLite: Executing {testPath} args: {arguments ?? "NULL"}");

		SetupProcessEnvironmentVariables();
        
		return base.Run(testPath, arguments);
	}

	private void SetupProcessEnvironmentVariables()
	{
        var traceLevel = CommandLineOptions.TraceLevel.Value ?? "Off";

        ProcessSettings.EnvironmentVariables = new Dictionary<string,string> {
            { "TESTCENTRIC_INTERNAL_TRACE_LEVEL", traceLevel }
        };
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
