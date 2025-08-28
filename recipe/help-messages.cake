static public class HelpMessages
{
    static private string HEADER = $"""
        TestCentric.Cake.Recipe - Version {Recipe.Version}
        """;

	static public string Usage => $"""
        {HEADER}

        TestCentric.Cake.Recipe is a set of scripts used to build this project. The
        recipe provides a number of built-in options and tasks. Additional options
        and tasks may be defined in your build.cake file or in other files it loads.

        Usage: build [options]

        Options:

            --target, -t=TARGET
                The TARGET task to be run, e.g. Test. Default is Build. This option
                may be repeated to run multiple targets. For a list of supported
                targets, use "-t Help --tasks".

            --configuration, -c=CONFIG
                The name of the configuration to build. Default is Release.

            --packageVersion, --package, --pv, -p=VERSION
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

            --level, --lev, -l=LEVEL
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

            --tasks
                Used with the Help task to indicate that the list of available tasks
                (targets) should be displayed.
        
            --usage
                Used with the Help task to indicate that the usage message should
                be displayed. Snce this is the default for Help, it is not normally
                needed but is provided in case the default changes in the future.

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

	static public string Tasks => $"""
        {HEADER}

        TestCentric.Cake.Recipe supports a number of tasks used to build, test,
        package, publish and release your application. Tasks to be run are specified
        by use of the `--target` option or its short form `-t`.
        
        If you specify multiple targets, they are executed in the order in which
        you entered them. Dependent targets are executed only once per run.

        ==============================================================================
            Build Tasks
        ==============================================================================
        
        Build
          Compiles the code in your solution. If there is no solution in the
          project, the command is not available and an error is displayed.
        
          Depends On: Clean, Restore, CheckHeaders
        
        Clean
          Clean output directories for current config as well as the package directory.
        
        CleanPackageDirectory
          Clean the package directory.

        Restore
          Restore all packages referenced by the solution.

        CheckHeaders
          Check source files for valid copyright headers. Currently, only C# files
          are checked. Normally a standard TestCentric header is used but a project
          may specify a different header when initializing BuildSettings.

        CleanAll
          Clean all output directories and package directory. Delete all
          object directories.
        
        ==============================================================================
            Unit Testing Task
        ==============================================================================
        
        Test
          Does Build and then runs your unit tests if you have any. If you are
          certain that nothing in your code has changed, you can use `--nobuild` to
          eliminate the compilation step.
        
          Depends On: Build

        ==============================================================================
            Packaging Tasks
        ==============================================================================
        
        Package
          Builds, installs, verifies and tests all the packages you have defined.
          Verification is based on the checks you define for each package. Testing
          uses the tests you have specified for each package. If you are certain
          that nothing in your code has changed, you can use `--nobuild` to
          eliminate the compilation step.
        
          Depends On: Build
        
        BuildPackages
          Compiles your application and then builds the packages. Use --nobuild to skip
          compilation. Use for debugging the building of packages.
        
          Depends On: Build
        
        InstallPackages
          Builds and installs packages. Useful for debugging installation.
        
          Depends On: BuildPackages
        
        VerifyPackages
          Builds, Installs and Verifies packages. Useful for debugging package content.
        
          Depends On: InstallPackages
                
        TestPackages
          Builds, Installs and runs package tests. Particularly useful in combination
          with the --where option to debug a single package.
        
          Depends On: InstallPackages
        
        ==============================================================================
            Publishing Tasks
        ==============================================================================
        
        Publish
          Publishes packages to MyGet, NuGet or Chocolatey, based on the
          branch being built and the package version. Although this task
          is not dependent on the PublishToMyget, PublishToNuGet or
          PublishToChocolatey tasks, it calls the same underlying code
          used by those tasks.
        
          Depends On: Package Runs: 
        
        PublishToMyGet
          Publishes packages to MyGet for a dev build. If not, or if the --nopush
          option was used, a message is displayed. Used directly when publishing
          to MyGet has failed due to an external error.
                  
        PublishToNuGet
          Publishes packages to NuGet for an alpha, beta, rc or final release. If not,
          or if the --nopush option was used, a message is displayed. Used directly when
          publishing to NuGet has failed due to an external error.

        PublishToChocolatey
          Publishes packages to Chocolatey for an alpha, beta, rc or final release.
          If not, or if the --nopush option was used, a message is displayed. Used 
          directly when publishing to Chocolatey has failed due to an external error.
        
        PublishToLocalFeed
          Publishes packages to the local feed for a dev, alpha, beta, or rc build
          or for a final release. If not, or if the --nopush option was used,
          a message is displayed.        
        
        ==============================================================================
            Release Tasks
        ==============================================================================
        
        CreateDraftRelease
          Creates a draft release for a milestone on GitHub. The milestone name must
          match the three-part package version for each package. This target will fail
          with an error message if no milestone is found or if it doesn't meet criteria
          for a draft release.
            
        DownloadDraftRelease
          Download draft release for local use

        UpdateReleaseNotes
          Update Release Notes.
        
        CreateProductionRelease
          Creates a production release for a milestoneon GitHub. The milestone name
          must match the three-part package version for each package. This target will
          fail with an error message if no milestone is found or if it doesn't meet
          criteria for a production release.

        ==============================================================================
            Continuous Integration Task
        ==============================================================================
        
        ContinuousIntegration
          Perform a continuous integration run, using dependent tasks to build and 
          unit test the software, create, install, verify and test packages. If run
          on a release branch (release-x.y.z), it will also create a draft release.
          If run on main, it will publish the packages and create a full production
          release on GitHub assuming no failures occur.
        
          This task will normally only be run on a CI server. For a given release, 
          it should only be run on the CI server selected to perform releases. Other 
          targets must be selected for any additional serviers in use.        
        
          Depends On: Build, Test, Package, Publish,
                      CreateDraftRelease, CreateProductionRelease

        ==============================================================================
            Miscellaneous Tasks
        ==============================================================================
        
        CheckScript
          Verify that the script compiles.
        
        DumpSettings
          Display build settings so that you can verify that your script has
          initialized them correctly.
        
        Help
          Display help info for the recipe package. The default display shows general
          usage information, including available options. For a list of available
          targets, add the --tasks option.        
        
        Default
          Default target if none is specified on the command-line. This is normally set
          to Build but may be changed when calling BuildSettings.Initialize().
        
          Depends On: {BuildTasks.DefaultTask.Task.Dependencies.FirstOrDefault().Name}
                
        """;
}
