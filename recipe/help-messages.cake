static public class HelpMessages
{
	static public string Usage => $"""
        BUILD.CAKE

        This script builds the {BuildSettings.Title} project. It makes use of
        TestCentric.Cake.Recipe, which provides a number of built-in options and
        tasks. You may define additional options and tasks in build.cake or
        in additional cake files you load from build.cake.

        Usage: build [options]

        Options:

            --target, -t=TARGET
                The TARGET task to be run, e.g. Test. Default is Build. This option
                may be repeated to run multiple targets. For a list of supported
                targets, use the Cake `--description` option.

            --configuration, -c=CONFIG
                The name of the configuration to build. Default is Release.

            --packageVersion, -p=VERSION
                Specifies the full package version, including any pre-release
                suffix. If provided, this version is used directly in place of
                the default version calculated by the script.

            --where, -w=SELECTION
                Specifies a selction expression used to choose which packages
                to build and test, for use in debugging. Consists of one or
                more specifications, separated by '|' and '&'. Each specification
                is of the form "prop=value", where prop may be either id or type.
                Examples:
                    --where type=nuget
                    --where id=NUnit.Engine.Api
                    --where "type=nuget|type=choco"

            --level, -l=LEVEL
                Specifies the level of package testing, 1, 2 or 3. Defaults are
                  1 = for normal CI tests run every time you build a package
                  2 = for PRs and Dev builds uploaded to MyGet
                  3 = prior to publishing a release

            --trace, --tr=LEVEL
                Specifies the default trace level for this run. Values are Off,
                Error, Warning, Info or Debug. Default is value of environment
                variable TESTCENTRIC_INTERNAL_TRACE_LEVEL. If the variable
                is not set, default is Off.

            --nobuild, --nob
                Indicates that the Build task should not be run even if other
                tasks depend on it. The existing build is used instead.

            --nopush, --nop
                Indicates that no publishing or releasing should be done. If
                publish or release targets are run, a message is displayed.

            --usage, -u
                Used with the Help task to indicate that the usage message should
                be displayed. Currently "usage" is the only display for Help, so
                this is not needed but is provided for future use.

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
