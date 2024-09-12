//////////////////////////////////////////////////////////////////////
// UNIT TEST RUNNER
//////////////////////////////////////////////////////////////////////

public static class UnitTesting
{
    static ICakeContext _context;
    static IUnitTestRunner _runner;

    static UnitTesting()
    {
        _context = BuildSettings.Context;
        _runner = BuildSettings.UnitTestRunner ?? new NUnitLiteRunner();
    }

    public static void RunAllTests()
    {
		var unitTests = FindUnitTestFiles(BuildSettings.UnitTests);

        _context.Information($"Located {unitTests.Count} unit test assemblies.");
    
        foreach (var testPath in unitTests)
            RunTest(testPath);
    }

    private static void RunTest(FilePath testPath)
    {
        var testFile = testPath.GetFilename();
        var containingDir = testPath.GetDirectory().GetDirectoryName();
        var msg = "Running " + testFile;
        if (IsValidRuntime(containingDir))
            msg += " under " + containingDir;

        Banner.Display(msg);

        var runner = BuildSettings.UnitTestRunner ?? new NUnitLiteRunner();
        bool isNUnitLite = runner is NUnitLiteRunner;

        if (isNUnitLite)
            runner.RunUnitTest(testPath);
        else
		    runner.RunUnitTest($"{testPath} {BuildSettings.UnitTestArguments}");
		    
        var result = new ActualResult(BuildSettings.OutputDirectory + "TestResult.xml");

        if (!isNUnitLite) // NUnitLite displays it's own results
		    new ConsoleReporter(result).Display();

		if (result.OverallResult == "Failed")
			throw new System.Exception("There were test failures or errors. See listing.");

        static bool IsValidRuntime(string text)
        {
            string[] VALID_RUNTIMES = {
                "net20", "net30", "net35", "net40", "net45", "net451", "net451",
                "net46", "net461", "net462", "net47", "net471", "net472", "net48", "net481",
                "netcoreapp1.1", "netcoreapp2.1", "netcoreapp3.1",
                "net5.0", "net6.0", "net7.0", "net8.0"
            };

            return VALID_RUNTIMES.Contains(text);
        }
    }

    private static List<FilePath> FindUnitTestFiles(string patternSet)
    {
        var result = new List<FilePath>();

        if (!string.IsNullOrEmpty(patternSet))
        { 
            // User supplied a set of patterns for the unit tests
            foreach (string filePattern in patternSet.Split('|'))
                foreach (var testPath in _context.GetFiles(BuildSettings.OutputDirectory + filePattern))
                    result.Add(testPath);
        }
        else
        {
            // Use default patterns to find unit tests - case insensitive because
            // we don't know how the user may have named test assemblies.
            var defaultPatterns = _runner is NUnitLiteRunner
                ? new[] { "**/*.tests.exe" }
                : new[] { "**/*.tests.dll", "**/*.tests.exe" };
            var globberSettings = new GlobberSettings { IsCaseSensitive = false };
            foreach (string filePattern in defaultPatterns)
                foreach (var testPath in _context.GetFiles(BuildSettings.OutputDirectory + filePattern, globberSettings))
                    result.Add(testPath);
        }

        result.Sort(ComparePathsByFileName);

        return result;

        static int ComparePathsByFileName(FilePath x, FilePath y)
        {
            return x.GetFilename().ToString().CompareTo(y.GetFilename().ToString());
        }
    }
}
