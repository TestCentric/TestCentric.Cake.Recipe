# TestCentric.Cake.Recipe

TestCentric.Cake.Recipe is a standard cake recipe used for TestCentric projects. It is inspired by Cake.Recipe but is somewhat simpler in implementation, since it isn't intended for general use. The recipe is still under development.

## The `build.cs` File

Projects use the recipe from a [Cake.Sdk](https://cakebuild.net/docs/running-builds/runners/cake-sdk) build script, a single-file C# app named `build.cs`. This requires the .NET 10 SDK or later. The following is an example for a project which creates a single package.

```csharp
#:sdk Cake.Sdk@7.0.0
#:package TestCentric.Cake.Recipe@2.0.0

// Initialize Build Settings
BuildSettings.Initialize(
	context: Context,
	title: "Sample Application",
	githubRepository: "TestCentric.Sample.Application");

// Define the package
BuildSettings.Packages.Add(new NuGetPackage(
	id: "TestCentric.Sample.Application",
	source: "nuget/TestCentric.Sample.Application.nuspec",
	checks: new PackageCheck[] {
		HasFiles(
			"LICENSE.txt", "README.md", "testcentric.png",
			"lib/net8.0/testcentric.sample.application.dll") }));

// Run the task selected by user or default task
Build.Run();
```

Run it with `dotnet run --file build.cs -- <arguments>`, for example `dotnet run --file build.cs -- --target=Package`.

**NOTES:**

1. `#:package` references the recipe, specifying the version to use. It is generally recommended
   that you pin the version to avoid unpleasant surprises when the recipe is updated. The recipe's
   files are compiled into your build script, and the Cake addins it needs come with it.

2. `BuildSettings.Initialize` sets the parameters which drive the build process. The example
   specifies only the three required parameters.

3. Define a single `nuget` package.

4. Run the selected target. This must be the last statement in the file.

### A package README

nuget.org shows a package's description as plain text, but renders a package README as Markdown
on the package page. To add one, pass its path, relative to the project directory, as `readme`:

```csharp
BuildSettings.Packages.Add(new NuGetPackage(
	id: "TestCentric.Sample.Application",
	readme: "nuget/README.md",
	...
```

The recipe puts the file at the package root and names it in the nuspec's `readme` element. It
also adds a `HasReadMe` check, which verifies both. A package built entirely from a nuspec can
use `<readme>` there instead, and add `HasReadMe("README.md")` to its checks.

### Additional build files

Code in other files can be included with `#:property IncludeAdditionalFiles=file1.cs;file2.cs`.
Like the recipe's own files, each must be ordinary C#. Wrap its contents in
`public static partial class Program { ... }`, so its members are visible to `build.cs` and can
call Cake methods such as `Information` and `Task` directly.

If the project uses Central Package Management, add `#:property ManagePackageVersionsCentrally=false`
so that the `#:package` versions are accepted.

### Testing a local copy of the recipe

To build against a local checkout of the recipe instead of the package, remove the `#:package`
line for the recipe and include its files directly, for example:

```csharp
#:property IncludeAdditionalFiles=../TestCentric.Cake.Recipe/recipe/*.cs;PackageTests.cs
```

The recipe's addins then have to be referenced directly as well, with `#:package` lines for
`Cake.Git` and `Cake.MinVer`.

## Supported Arguments

The recipe supports a number of standard arguments for use in any project
that uses it. Additional arguments may be supported directly by the user's
build script if necessary but must not conflict with the built-in arguments.

Arguments taking a value may use  `=` or space to separate the name
from the value.

#### --target, -t=TARGET
The name of the TARGET task to be run, e.g. Test. Default is "Build."

#### --configuration, -c=CONFIG
The name of the configuration to build, test and/or package, e.g. Debug.
Defaults to Release.

#### --packageVersion, --package=VERSION
Specifies the full package version, including any pre-release
suffix. This version is used directly instead of the default
version from the script or that calculated by MinVer.
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

#### --trace=LEVEL
Specifies the default trace level for this run. Values are Off,
Error, Warning, Info or Debug. Default is value of environment
variable TESTCENTRIC_INTERNAL_TRACE_LEVEL if set. If not,
tracing is turned Off.

#### --nobuild
Indicates that the Build task should not be run even if other
tasks depend on it. The existing build is used instead.

#### --nopush
Indicates that no publishing or releasing should be done. If
publish or release targets are run, a message is displayed.

#### --usage
Displays this help message. No targets are run.
