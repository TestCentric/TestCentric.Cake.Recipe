static public class HelpMessages
{
	static public string Summary => $"""
        This script builds the {BuildSettings.Title} project. It makes use of
        TestCentric.Cake.Recipe, which provides a number of built-in options and
        tasks. You may define additional options and tasks in build.cake.

        Usage: build [options]

        {Options}

        {BuildTasks.TaskList}
        """;

    public static string Options = $"""
        Options:

            --target, -t=TARGET
                The TARGET task to be run, e.g. Test. Specify Default in `build.cake`.

            --configuration, -c=CONFIG
                The name of the configuration to build. Default is Release.

            --packageVersion, --package=VERSION
                Specifies the full package version, including any pre-release suffix.
                This version is used directly instead of the default determined by
                the script. All other versions (AssemblyVersion, etc.) derive from this.

              NOTE: We can't use "--version" since that's an argument to Cake itself.

            --testLevel, --level=LEVEL
                Specifies the level of package testing, 1, 2 or 3. Defaults are
                  1 = for normal CI tests run every time you build a package
                  2 = for PRs and Dev builds uploaded to MyGet
                  3 = prior to publishing a release

            --trace=LEVEL
                Specifies the default trace level for this run. Values are Off, Error,
                Warning, Info or Debug. Default is taken from the environment variable
                TESTCENTRIC_INTERNAL_TRACE_LEVEL if set, otherwise Off.

            --nobuild
                Indicates that the Build task should not be run even if the target
                task normally depends on it. The existing build is used.

            --nopush
                Indicates that no publishing or releasing should be done. If
                publish or release targets are run, a message is displayed.
        """;
}
