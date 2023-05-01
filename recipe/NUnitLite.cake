public static class NUnitLite
{
    private static readonly string[] VALID_RUNTIMES = new [] {
        "net20", "net30", "net35", "net40", "net45", "net451", "net451",
        "net46", "net461", "net462", "net47", "net471", "net472", "net48", "net481",
        "netcoreapp1.1", "netcoreapp2.1", "netcoreapp3.1",
        "net5.0", "net6.0", "net7.0", "net8.0"
    };

    private static ICakeContext _context;
    private static List<string> _errors = new List<string>();

    static NUnitLite()
    {
        _context = BuildSettings.Context;
    }

    public static void RunUnitTests(string patternSet = null)
    {
        _errors.Clear();

        if (patternSet == null)
            patternSet = BuildSettings.UnitTests;

        _context.Information("Running NUnitLite Tests matching " + (patternSet ?? "default pattern \"**/*.tests.dll|**/*.tests.exe\""));
        foreach (var testPath in GetUnitTestFiles(patternSet))
        {
		    int rc = Run(testPath);

            if (rc != 0)
                RecordError(testPath, rc);                
        }

        if (_errors.Count > 0)
        {
            string msg = string.Join(Environment.NewLine, _errors.ToArray());
            throw new Exception(msg);
        }
    }

    private static void RecordError(FilePath testPath, int rc)
    {        
        var testFile = testPath.GetFilename();
        var containingDir = testPath.GetDirectory().GetDirectoryName();

        string msg = rc > 0
            ? $"{testFile} had {rc} errors"
            : $"{testFile} returned {rc}";

        if (IsValidRuntime(containingDir))
            msg += " under " + containingDir;

        _errors.Add(msg);
    }

	private static int Run(FilePath testPath)
    {
        var testFile = testPath.GetFilename();
        var containingDir = testPath.GetDirectory().GetDirectoryName();
        var msg = "Running " + testFile;
        if (IsValidRuntime(containingDir))
            msg += " under " + containingDir;

        Banner.Display(msg);

        // NUnitLite tests with a .dll extension are run under the dotnet CLI
        if (testPath.GetExtension() == ".dll")
            return _context.StartProcess("dotnet", new ProcessSettings { Arguments = testPath.ToString() });
        else
            return _context.StartProcess(testPath);
    }

    private static bool IsValidRuntime(string text) => VALID_RUNTIMES.Contains(text);

    private static List<FilePath> GetUnitTestFiles(string patternSet)
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
            var defaultPatterns = new [] { "**/*.tests.dll", "**/*.tests.exe" };
            var globberSettings = new GlobberSettings { IsCaseSensitive = false };
            foreach (string filePattern in defaultPatterns)
                foreach (var testPath in _context.GetFiles(BuildSettings.OutputDirectory + filePattern, globberSettings))
                    result.Add(testPath);
        }

        result.Sort(ComparePathsByFileName);

        return result;
    }

    static int ComparePathsByFileName(FilePath x, FilePath y)
    {
        return x.GetFilename().ToString().CompareTo(y.GetFilename().ToString());
    }
}

private int RunNUnitLite(string testName, string runtime, string directory)
{
    bool isDotNetCore = runtime.StartsWith("netcoreapp");
    string ext = isDotNetCore ? ".dll" : ".exe";
    string testPath = directory + testName + ext;

    Information("==================================================");
    Information("Running tests under " + runtime);
    Information("==================================================");

    int rc = isDotNetCore
        ? StartProcess("dotnet", testPath)
        : StartProcess(testPath);

    return rc;
}
