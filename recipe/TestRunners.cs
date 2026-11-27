// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
/// <summary>
/// Common interface for all test runners
/// </summary>
public interface ITestRunner
{
	string PackageId { get; }
	string Version { get; }
}

/// <summary>
/// A runner capable of running unit tests
/// </summary>
public interface IUnitTestRunner : ITestRunner
{
	int RunUnitTest(FilePath testPath);
}

/// <summary>
/// A runner capable of running package tests
/// </summary>
public interface IPackageTestRunner : ITestRunner
{
	int RunPackageTest(string arguments);
}

/// <summary>
/// The TestRunner class is the abstract base for all TestRunners used to run unit-
/// or package-tests. A TestRunner knows how to run a test assembly and provide a result.
/// All base functionality is implemented in this class. Derived classes make that
/// functionality available selectively by implementing specific interfaces.
/// </summary>
public abstract class TestRunner : ITestRunner
{
	protected ICakeContext Context => BuildSettings.Context;

	public string PackageId { get; protected set; }
	public string Version { get; protected set; }

	protected int RunTest(FilePath executablePath, string arguments = null)
	{
		return RunTest(executablePath, new ProcessSettings { Arguments = arguments });
	}

	protected int RunTest(FilePath executablePath, ProcessSettings processSettings=null)
	{
		if (executablePath == null)
			throw new ArgumentNullException(nameof(executablePath));

		if (processSettings == null)
			processSettings = new ProcessSettings();

		// Add default values to settings if not present
		if (processSettings.WorkingDirectory == null)
			processSettings.WorkingDirectory = BuildSettings.OutputDirectory;

		if (executablePath.GetExtension() == ".dll")
		{
			processSettings.Arguments = $"\"{executablePath}\" {processSettings.Arguments}";
			return Context.StartProcess("dotnet", processSettings);
		}
        else
			return Context.StartProcess(executablePath, processSettings);
    }
}

/// <summary>
/// A TestRunner requiring some sort of installation before use.
/// </summary>
public abstract class InstallableTestRunner : TestRunner
{
	protected InstallableTestRunner(string packageId, string version)
	{
		PackageId = packageId;
		Version = version;
	}

	protected abstract FilePath ExecutableRelativePath { get; }

	// Path under tools directory where package would be installed by Cake #tool directive.
	// NOTE: When used to run unit tests, a #tool directive is required. If derived package
	// is only used for package tests, it is optional.
	protected DirectoryPath ToolInstallDirectory => BuildSettings.ToolsDirectory + $"{PackageId}.{Version}"; 
	protected bool IsInstalledAsTool =>
		ToolInstallDirectory != null && Context.DirectoryExists(ToolInstallDirectory);
	
	protected DirectoryPath InstallDirectory;

	public FilePath ExecutablePath => InstallDirectory.CombineWithFilePath(ExecutableRelativePath);

	public void Install(DirectoryPath installDirectory)
	{
		InstallDirectory = installDirectory.Combine($"{PackageId}.{Version}");

		// If the runner package is already installed as a cake tool, we just copy it
		if (IsInstalledAsTool)
			Context.CopyDirectory(ToolInstallDirectory, InstallDirectory);
		// Otherwise, we install it to the requested location
		else
			Context.NuGetInstall(
				PackageId,
				new NuGetInstallSettings() { OutputDirectory = installDirectory, Version = Version });
	}
}

/// <summary>
/// Runs NUnitLite unit tests. The test assembly is itself the runner, so it is run directly.
/// </summary>
public class NUnitLiteRunner : TestRunner, IUnitTestRunner
{
	public int RunUnitTest(FilePath testPath)
	{
		var processSettings = new ProcessSettings { Arguments = BuildSettings.UnitTestArguments };
		if (CommandLineOptions.TraceLevel.Exists)
			processSettings.EnvironmentVariables = new Dictionary<string,string>
			{
				{ "TESTCENTRIC_INTERNAL_TRACE_LEVEL", CommandLineOptions.TraceLevel.Value }
			};
        
		return RunTest(testPath, processSettings);
	}
}

/// <summary>
/// Runs unit and package tests with the NUnit console runner, which is installed in the tools directory before use.
/// </summary>
public class NUnitConsoleRunner : InstallableTestRunner, IUnitTestRunner, IPackageTestRunner
{
	protected override FilePath ExecutableRelativePath => "tools/nunit3-console.exe";
	
	public NUnitConsoleRunner(string version) : base("NUnit.ConsoleRunner", version) { }

	// Run a unit test
	public int RunUnitTest(FilePath testPath) => RunTest(ToolInstallDirectory.CombineWithFilePath(ExecutableRelativePath), $"\"{testPath}\" {BuildSettings.UnitTestArguments}");

	// Run a package test
	public int RunPackageTest(string arguments) => RunTest(ExecutablePath, arguments);
}

/// <summary>
/// Runs unit and package tests with the .NET Core build of the NUnit console runner.
/// </summary>
public class NUnitNetCoreConsoleRunner : InstallableTestRunner, IUnitTestRunner, IPackageTestRunner
{
	protected override FilePath ExecutableRelativePath => "tools/net6.0/nunit3-console.exe";

	public NUnitNetCoreConsoleRunner(string version) : base("NUnit.ConsoleRunner.NetCore", version) { }

	// Run a unit test
	public int RunUnitTest(FilePath testPath) => RunTest(ExecutablePath, $"\"{testPath}\" {BuildSettings.UnitTestArguments}");

	// Run a package test
	public int RunPackageTest(string arguments) => RunTest(ExecutablePath, arguments);
}

/// <summary>
/// Class that knows how to run an agent directly.
/// </summary>
public class AgentRunner : TestRunner, IPackageTestRunner
{
    private FilePath _stdExecutable;
    private FilePath _x86Executable;

	public AgentRunner(FilePath stdExecutable, FilePath x86Executable = null)
	{
        _stdExecutable = stdExecutable;
        _x86Executable = x86Executable;
    }

    public int RunPackageTest(string arguments)
    {
		FilePath executablePath = arguments.Contains("--x86")
            ? _x86Executable
            : _stdExecutable;
		arguments = arguments.Replace("--x86", string.Empty);

		if (executablePath.GetExtension() == ".dll")
			return RunTest("dotnet", $"\"{executablePath}\" {arguments}");
		else
			return base.RunTest(executablePath, arguments);
	}
}
}
