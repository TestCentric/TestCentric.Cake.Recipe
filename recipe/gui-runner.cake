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
	}

	protected override FilePath ExecutableRelativePath => "tools/testcentric.exe";

	public string BuiltInAgentUnderTest { get; set; }

	public int Run(string arguments)
	{
		if (string.IsNullOrEmpty(arguments))
			throw new ArgumentException("No run arguments supplied");

		if (!arguments.Contains(" --run"))
			arguments += " --run";
		if (!arguments.Contains(" --unattended"))
			arguments += " --unattended";

		return RunTest(ExecutablePath, arguments);
	}
}
