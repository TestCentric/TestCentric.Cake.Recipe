using System.Runtime.Versioning;

public class PluggableAgentFactory
{
	private const string GUI_VERSION = "2.0.0-beta1";
	private const string README = "../../README.md";
	private const string LICENSE = "../../LICENSE.txt";
	private const string ICON = "../../testcentric.png";
	private const string CHOCO_VERIFICATION = "../../VERIFICATION.txt";

	// Agents which are included in the Gui 2.0.0-beta1 distribution
	private static readonly AgentInfo[] BuiltInAgents = new AgentInfo[] {
		new AgentInfo("Net462AgentLauncher", new FrameworkName(".NetFramework, Version=4.6.2")),
		new AgentInfo("Net60AgentLauncher", new FrameworkName(".NetCoreApp, Version=6.0.0")),
		new AgentInfo("Net70AgentLauncher", new FrameworkName(".NetCoreApp, Version=7.0.0"))
	};

	private struct AgentInfo
	{
		public string LauncherName { get; }
		public FrameworkName TargetFramework { get; }
		public bool IsNetCore => TargetFramework.Identifier == ".NetCoreApp";
		public bool IsNetFramework => TargetFramework.Identifier == ".NetFramework";

		public AgentInfo(string launcherName, FrameworkName targetFramework)
		{
			LauncherName = launcherName;
			TargetFramework = targetFramework;
		}
	}

	private static SortedList<Version, AgentInfo> AvailableAgents = new SortedList<Version, AgentInfo>();

	// Set in constructor
	private FrameworkName TargetFramework { get; }
	private string NuGetId { get; }
	private string ChocoId { get; }
	private string TargetLauncherName { get; }
	private string TargetAgentName { get; }
	private string TargetLauncherFileNameWithoutExtension { get; }
	private string TargetLauncherFileName { get; }
	private string TargetAgentFileNameWithoutExtension { get; }
	private string TargetAgentFileName { get; }
	private string Title { get; }
	private string Description { get; }
	private string[] Tags { get; }

	private string TargetIdentifier =>TargetFramework.Identifier;
	private Version TargetVersion => TargetFramework.Version;
	private string TargetVersionWithoutDots => TargetVersion.ToString().Replace(".", "");
	
	private bool TargetIsNetFramework => TargetIdentifier == ".NetFramework";
	private bool TargetIsNetCore => TargetIdentifier == ".NetCoreApp";
	private bool TargetIsBuiltInAgent { get; }
	
	private FilePath[] LauncherFiles => new FilePath[] {
		TargetLauncherFileName, TargetLauncherFileNameWithoutExtension + ".pdb", "nunit.engine.api.dll", "testcentric.engine.api.dll" };
	private FilePath[] AgentFiles => TargetIsNetCore
		? new FilePath[] {
			$"agent/{TargetAgentFileName}", $"agent/{TargetAgentFileNameWithoutExtension}.pdb", $"agent/{TargetAgentFileName}.config",
			$"agent/{TargetAgentFileNameWithoutExtension}.deps.json", $"agent/{TargetAgentFileNameWithoutExtension}.runtimeconfig.json",
			"agent/nunit.engine.api.dll", "agent/testcentric.engine.core.dll", "agent/testcentric.engine.metadata.dll",
			"agent/testcentric.extensibility.dll", "agent/Microsoft.Extensions.DependencyModel.dll" }
		: new FilePath[] {
			$"agent/{TargetAgentFileName}", $"agent/{TargetAgentFileNameWithoutExtension}.pdb", $"agent/{TargetAgentFileName}.config",
			"agent/nunit.engine.api.dll", "agent/testcentric.engine.core.dll", "agent/testcentric.engine.metadata.dll", "agent/testcentric.extensibility.dll" };

	private List<PackageTest> PackageTests = new List<PackageTest>();

	/// <summary>
	/// Construct a factory for a particular runtime
	/// </summary>
	/// <param name="targetFramework">String in the form of a target framework moniker representing the target runtime for this factory.</param>
	public PluggableAgentFactory(string targetFramework)
	{
		TargetFramework = new FrameworkName(targetFramework);

		if (TargetIsNetFramework)
		{
			NuGetId = $"NUnit.Extension.Net{TargetVersionWithoutDots}PluggableAgent";
			ChocoId = $"nunit-extension-net{TargetVersionWithoutDots}-pluggable-agent";
			Title = $".NET {TargetVersion} Pluggable Agent";
			Description = $"TestCentric engine extension for running tests under .NET {TargetVersion}";
			TargetLauncherName = $"Net{TargetVersionWithoutDots}AgentLauncher";
			TargetLauncherFileNameWithoutExtension = $"net{TargetVersionWithoutDots}-agent-launcher";
			TargetAgentFileNameWithoutExtension = $"net{TargetVersionWithoutDots}-pluggable-agent";
			TargetAgentFileName = TargetAgentFileNameWithoutExtension + ".exe";
			Tags = new [] { "testcentric", "pluggable", "agent", $"net{TargetVersionWithoutDots}" };
		}
		else
		{
			if (TargetVersion.Major <= 3)
			{
				NuGetId = $"NUnit.Extension.NetCore{TargetVersionWithoutDots}PluggableAgent";
				ChocoId = $"nunit-extension-netcore{TargetVersionWithoutDots}-pluggable-agent";
				Title = $".NET Core {TargetVersion} Pluggable Agent";
				Description = $"TestCentric engine extension for running tests under .NET Core {TargetVersion}";
				TargetLauncherName = $"NetCore{TargetVersionWithoutDots}AgentLauncher";
				TargetLauncherFileNameWithoutExtension = $"netcore{TargetVersionWithoutDots}-agent-launcher";
				TargetAgentFileNameWithoutExtension = $"netcore{TargetVersionWithoutDots}-pluggable-agent";
				Tags = new [] { "testcentric", "pluggable", "agent", $"netcoreapp{TargetVersion}" };
			}
			else
			{
				NuGetId = $"NUnit.Extension.Net{TargetVersionWithoutDots}PluggableAgent";
				ChocoId = $"nunit-extension-net{TargetVersionWithoutDots}-pluggable-agent";
				Title = $".NET {TargetVersion} Pluggable Agent";
				Description = $"TestCentric engine extension for running tests under .NET {TargetVersion}";
				TargetLauncherName = $"Net{TargetVersionWithoutDots}AgentLauncher";
				TargetLauncherFileNameWithoutExtension = $"net{TargetVersionWithoutDots}-agent-launcher";
				TargetAgentFileNameWithoutExtension = $"net{TargetVersionWithoutDots}-pluggable-agent";
				Tags = new [] { "testcentric", "pluggable", "agent", $"net{TargetVersion}" };
			}

			TargetAgentFileName = TargetAgentFileNameWithoutExtension + ".dll";
		}

		TargetLauncherFileName = TargetLauncherFileNameWithoutExtension + ".dll";

		// Add all agents of the correct type (.NET Framework or .NET Core) to AvailableAgents
		foreach (var agent in BuiltInAgents)
			if (agent.IsNetFramework == TargetIsNetFramework)
			{
				AvailableAgents.Add(agent.TargetFramework.Version, agent);
				if (agent.TargetFramework == TargetFramework)
					TargetIsBuiltInAgent = true;
			}

		if (!TargetIsBuiltInAgent)
			AvailableAgents.Add(TargetVersion, new AgentInfo(TargetLauncherName, TargetFramework));

		DefinePackageTests();
	}

	public NuGetPackage NuGetPackage =>
		new NuGetPackage(
			NuGetId,
			title: Title,
			description: Description,
			tags: Tags,
			packageContent: new PackageContent(
				new FilePath[] { LICENSE, README, ICON },
				new DirectoryContent("tools").WithFiles( LauncherFiles ),
				new DirectoryContent("tools/agent").WithFiles( AgentFiles )),
			testRunner: TargetIsBuiltInAgent
				? new GuiRunner("TestCentric.GuiRunner", GUI_VERSION) { BuiltInAgentUnderTest = NuGetId }
				: new GuiRunner("TestCentric.GuiRunner", GUI_VERSION),
			tests: PackageTests);
	
	public ChocolateyPackage ChocolateyPackage =>
		new ChocolateyPackage(
			ChocoId,
			title: Title,
			description: Description,
			tags: Tags,
			packageContent: new PackageContent(
				new FilePath[] { ICON },
				new DirectoryContent("tools").WithFiles( LICENSE, README, CHOCO_VERIFICATION ).AndFiles( LauncherFiles ),
				new DirectoryContent("tools/agent").WithFiles( AgentFiles )),
			testRunner: TargetIsBuiltInAgent
				? new GuiRunner("testcentric-gui", GUI_VERSION) { BuiltInAgentUnderTest = ChocoId }
				: new GuiRunner("testcentric-gui", GUI_VERSION),
			tests: PackageTests);

	public PackageDefinition[] Packages => new PackageDefinition[] { NuGetPackage, ChocolateyPackage };

	// Define Package Tests for both packages
	//   Level 1 tests are run each time we build the packages
	//   Level 2 tests are run for PRs and when packages will be published
	//   Level 3 tests are run only when publishing a release

	private void DefinePackageTests()
	{
		// NOTE: Because we are comparing versions using > and <, it's important that all
		// versions be specified using the same number of components. Except for 4.6.2,
		// we use two components. Version 4.6.2 will be a problem to solve if we want to 
		// create a 4.8.0 version in the future

		if (TargetIsNetFramework)
		{
			if (TargetVersion >= new Version(2,0))
			{
				PackageTests.Add(new PackageTest(
					1, "Net20PackageTest", "Run mock-assembly.dll targeting .NET 2.0",
					"tests/net20/mock-assembly.dll", MockAssemblyResult(new Version(2,0))));
			
				PackageTests.Add(new PackageTest(
					1, "Net35PackageTest", "Run mock-assembly.dll targeting .NET 3.5",
					"tests/net35/mock-assembly.dll", MockAssemblyResult(new Version(3,5))));
			}

			if (TargetVersion >= new Version(4,6,2))
				PackageTests.Add(new PackageTest(
					1, "Net462PackageTest", "Run mock-assembly.dll targeting .NET 4.6.2",
					"tests/net462/mock-assembly.dll", MockAssemblyResult(new Version(4,6,2))));
		}
		else if (TargetIsNetCore)
		{
			if (TargetVersion >= new Version(1,1))
				PackageTests.Add(new PackageTest(
					1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
					"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult(new Version(1,1))));

			if (TargetVersion >= new Version(2,1))
				PackageTests.Add(new PackageTest(
					1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
					"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult(new Version(2,1))));

			if (TargetVersion >= new Version(3,1))
				PackageTests.Add(new PackageTest(
					1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
					"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult(new Version(3,1))));

			if (TargetVersion >= new Version(5,0))
				PackageTests.Add(new PackageTest(
					1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
					"tests/net5.0/mock-assembly.dll", MockAssemblyResult(new Version(5,0))));

			if (TargetVersion >= new Version(6,0))
				PackageTests.Add(new PackageTest(
					1, "Net60PackageTest", "Run mock-assembly.dll targeting .NET 6.0",
					"tests/net6.0/mock-assembly.dll", MockAssemblyResult(new Version(6,0))));

			if (TargetVersion >= new Version(7,0))
				PackageTests.Add(new PackageTest(
					1, "Net70PackageTest", "Run mock-assembly.dll targeting .NET 7.0",
					"tests/net7.0/mock-assembly.dll", MockAssemblyResult(new Version(7,0))));

			// Special handling for target version > highest built-in version
			if (TargetVersion > new Version(7,0))
				PackageTests.Add(new PackageTest(
					1, $"Net{TargetVersionWithoutDots}PackageTest", $"Run mock-assembly.dll targeting .NET {TargetVersion}",
					$"tests/net{TargetVersion}/mock-assembly.dll", MockAssemblyResult(TargetVersion)));

			// Run AspNetCore test for target framework >= 3.1
			if (TargetVersion == new Version(3,1))
				PackageTests.Add(new PackageTest(
					1, $"AspNetCore{TargetVersionWithoutDots}Test", $"Run test using AspNetCore targeting .NET {TargetVersion}",
					$"tests/netcoreapp{TargetVersion}/aspnetcore-test.dll", AspNetCoreResult(TargetVersion)));

			if (TargetVersion > new Version(3,1))
				PackageTests.Add(new PackageTest(
					1, $"AspNetCore{TargetVersionWithoutDots}Test", $"Run test using AspNetCore targeting .NET {TargetVersion}",
					$"tests/net{TargetVersion}/aspnetcore-test.dll", AspNetCoreResult(TargetVersion)));

			// Run Windows test for target framework >= 5.0 (6.0 on AppVeyor)
			if (TargetVersion >= new Version(6,0) || TargetVersion >= new Version(5,0) && !BuildSettings.IsRunningOnAppVeyor)
				PackageTests.Add(new PackageTest(
					1, $"Net{TargetVersionWithoutDots}WindowsFormsTest", $"Run test using windows forms under .NET {TargetVersion}",
					$"tests/net{TargetVersion}-windows/windows-forms-test.dll", WindowsFormsResult(TargetVersion)));
		}
	}

	// Define expected results
	private ExpectedResult MockAssemblyResult(Version testVersion) => new ExpectedResult("Failed")
	{
		Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
		Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll", ExpectedLauncher(testVersion)) }
	};

	private ExpectedResult AspNetCoreResult(Version testVersion) => new ExpectedResult("Passed")
	{
		Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
		Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("aspnetcore-test.dll", ExpectedLauncher(testVersion)) }
	};

	private ExpectedResult WindowsFormsResult(Version testVersion) => new ExpectedResult("Passed")
	{
		Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
		Assemblies = new ExpectedAssemblyResult[] {	new ExpectedAssemblyResult("windows-forms-test.dll", ExpectedLauncher(testVersion)) }
	};

	private string ExpectedLauncher(Version testVersion)
	{
		// Special handling for net20 agent
		if (TargetIsNetFramework && TargetVersion == new Version(2,0) && testVersion <= new Version(3,5))
			return "Net20AgentLauncher";
		
		foreach (var entry in AvailableAgents)
		{
			AgentInfo agent = entry.Value;

			if (TargetVersion <= agent.TargetFramework.Version && TargetVersion >= testVersion)
				return TargetLauncherName;
			if (testVersion <= agent.TargetFramework.Version)
				return agent.LauncherName;
		}

		// TargetVersion is greater than any builtin, so we must be testing a new, higher-version agent
		return TargetLauncherName;
	}
}
