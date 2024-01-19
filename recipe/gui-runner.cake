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
