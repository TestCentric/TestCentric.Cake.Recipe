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
            { "TESTCENTRIC_INTERNAL_TRACE", traceLevel }
        };
        
        return base.Run(arguments);
    }
}
