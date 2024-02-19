static public class HelpMessages
{
	static public string Usage => $"""
        BUILD.CAKE

        This script builds the {BuildSettings.Title} project. It makes use of
        TestCentric.Cake.Recipe, which provides a number of built-in options and
        tasks. You may define additional options and tasks in build.cake.

        Usage: build [options]

        Options:

            --target, -t=TARGET
                The TARGET task to be run, e.g. Test. Default is Build"

            --configuration, -c=CONFIG
                The name of the configuration to build. Default is Release.

            --packageVersion, --package=VERSION
                Specifies the full package version, including any pre-release
                suffix. If provided, this version is used directly in place of
                the default version calculated by the script.

            --testLevel, --level=LEVEL
                Specifies the level of package testing, 1, 2 or 3. Defaults are
                  1 = for normal CI tests run every time you build a package
                  2 = for PRs and Dev builds uploaded to MyGet
                  3 = prior to publishing a release

            --trace=LEVEL
                Specifies the default trace level for this run. Values are Off,
                Error, Warning, Info or Debug. Default is value of environment
                variable TESTCENTRIC_INTERNAL_TRACE_LEVEL if set. If not,
                tracing is turned Off.

            --nobuild
                Indicates that the Build task should not be run even if other
                tasks depend on it. The existing build is used instead.

            --nopush
                Indicates that no publishing or releasing should be done. If
                publish or release targets are run, a message is displayed.

            --usage
                Displays this help message. No targets are run.

        Selected Cake Options:
            
            --version
                Displays the cake version in use.

            --description
                Displays a list of the available tasks (targets).

            --tree
                Displays the task dependency tree

            --help
                Displays help information for cake itself.

            NOTE: The above Cake options bypass execution of the script.
        """;
}
