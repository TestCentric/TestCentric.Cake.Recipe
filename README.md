# TestCentric.Cake.Recipe

TestCentric.Cake.Recipe is a standard cake recipe used for TestCentric projects.
It is inspired by Cake.Recipe but is somewhat simpler in implementation, since
it isn't intended for general use. The recipe is still under development.

## Usage

This section is under development. For now, take a look at the `net20-pluggable-agent`
project for an example of how the recipe is used.

## Supported Arguments

The recipe supports a number of standard arguments for use in any project
that uses it. Additional arguments may be supported directly by the user's
`build.cake` if necessary but must not conflict with the built-in arguments.

Arguments taking a value may use  `=` or space to separate the name
from the value.

#### --target, -t=TARGET
The name of the TARGET task to be run, e.g. Test. Default is specified in `build.cake`.

#### --configuration, -c=CONFIG
The name of the configuration to build, test and/or package, e.g. Debug.
Defaults to Release.

#### --packageVersion, --package=VERSION
Specifies the full package version, including any pre-release
suffix. This version is used directly instead of the default
version from the script or that calculated by GitVersion.
Note that all other versions (AssemblyVersion, etc.) are
derived from the package version.

NOTE: We can't use "version" since that's an argument to Cake itself.

#### --testLevel, --level=LEVEL
Specifies the level of package testing, which is normally set
automatically for different types of builds like CI, PR, etc.
Used by developers to test packages locally without creating
a PR or publishing the package. Defined levels are
  1. Normal CI tests run every time you build a package
  2. Adds more tests for PRs and Dev builds uploaded to MyGet
  3. Adds even more tests prior to publishing a release

#### --nopush
Indicates that no publishing or releasing should be done. If
publish or release targets are run, a message is displayed.
