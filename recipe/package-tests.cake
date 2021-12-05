public abstract class PackageTester
{
    const string TEST_RESULT = "TestResult.xml";

    static readonly ExpectedResult EXPECTED_RESULT = new ExpectedResult("Failed")
    {
        Total = 36,
        Passed = 23,
        Failed = 5,
        Warnings = 1,
        Inconclusive = 1,
        Skipped = 7,
        Assemblies = new AssemblyResult[]
        {
            new AssemblyResult() { Name = "mock-assembly.dll" }
        }
    };

    protected BuildSettings _settings;
    protected ICakeContext _context;
    protected GuiRunner _guiRunner;

    public PackageTester(BuildSettings settings)
    {
        _settings = settings;
        _context = settings.Context;
    }

    protected abstract string PackageId { get; }
    protected abstract string RunnerId { get; }
    
    public void RunAllTests(params string[] runtimes)
    {
        _guiRunner.InstallRunner();

        int errors = 0;
        foreach (var runtime in runtimes)
        {
            _context.Information($"Running {runtime} mock-assembly tests");

            var actual = RunTest(runtime);

            var report = new TestReport(EXPECTED_RESULT, actual);
            errors += report.Errors.Count;
            report.DisplayErrors();
        }

        if (errors > 0)
            throw new System.Exception("A package test failed!");
    }

    private ActualResult RunTest(string runtime)
    {
        // Delete result file ahead of time so we don't mistakenly
        // read a left-over file from another test run. Leave the
        // file after the run in case we need it to debug a failure.
        if (_context.FileExists(_settings.OutputDirectory + TEST_RESULT))
            _context.DeleteFile(_settings.OutputDirectory + TEST_RESULT);

        _guiRunner.RunUnattended($"{_settings.OutputDirectory}tests/{runtime}/mock-assembly.dll");

        return new ActualResult(_settings.OutputDirectory + TEST_RESULT);
    }
}

public class NuGetPackageTester : PackageTester
{
    public NuGetPackageTester(BuildSettings settings) : base(settings)
    {
        _guiRunner = new GuiRunner(settings, GuiRunner.NuGetId);
    }

    protected override string PackageId => _settings.NuGetId;
    protected override string RunnerId => GuiRunner.NuGetId;
}

public class ChocolateyPackageTester : PackageTester
{
    public ChocolateyPackageTester(BuildSettings settings) : base(settings)
    {
        _guiRunner = new GuiRunner(settings, GuiRunner.ChocoId);
    }

    protected override string PackageId => _settings.ChocoId;
    protected override string RunnerId => GuiRunner.ChocoId;
}
