/// <summary>
/// The TestRunner class is the abstract base for all TestRunners used to test packages.
/// A TestRunner knows how to run a test assembly and provide a result.
/// </summary>
public abstract class TestRunner
{
	protected string ExecutablePath { get; set; }

	public virtual int Run(string arguments)
	{
		if (ExecutablePath == null)
			throw new InvalidOperationException("Unable to run tests. Executable path has not been set.");

		int rc = BuildSettings.Context.StartProcess(ExecutablePath, new ProcessSettings()
		{
			Arguments = arguments,
			WorkingDirectory = BuildSettings.OutputDirectory
		});

		return rc;
	}
}

/// <summary>
/// The InstallableTestRunner class is the abstract base for TestRunners which
/// must be installed using a published package before they can be used.
/// </summary>
public abstract class InstallableTestRunner : TestRunner
{
	protected bool _installationRequired = true;

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

	public override int Run(string arguments)
	{
		if (_installationRequired)
			PerformInstallation();

		return base.Run(arguments);
	}

	protected abstract void PerformInstallation();
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

	protected override void PerformInstallation()
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
	}
}
