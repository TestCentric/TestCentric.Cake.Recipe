#tool nuget:?package=GitVersion.CommandLine&version=5.0.0

//////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////

const string NUGET_ID = "TestCentric.Cake.Recipe";
const string DEFAULT_VERSION = "0.1.0";
const string NUGET_DIR = "nuget/";
const string PACKAGE_DIR = "package/";
const string RECIPE_DIR = "recipe/";

const string MYGET_API_KEY = "MYGET_API_KEY";
const string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS  
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

var PackageVersion = Argument("version", DEFAULT_VERSION);
var NuGetPackageSource = $"{NUGET_DIR}{NUGET_ID}.nuspec";
var NuGetPackage = $"{PACKAGE_DIR}{NUGET_ID}.{PackageVersion}.nupkg";

//////////////////////////////////////////////////////////////////////
// BUILD PACKAGE
//////////////////////////////////////////////////////////////////////

Task("Package")
	.Does(() =>
	{
		CreateDirectory(PACKAGE_DIR);

		NuGetPack(NuGetPackageSource, new NuGetPackSettings()
		{
			Version = PackageVersion,
			OutputDirectory = PACKAGE_DIR,
			NoPackageAnalysis = true
		});
	});

//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGE
//////////////////////////////////////////////////////////////////////

Task("Publish")
	.IsDependentOn("Package")
	.Does(() =>
	{
		NuGetPush(NuGetPackage, new NuGetPushSettings()
		{
			ApiKey = EnvironmentVariable(MYGET_API_KEY),
			Source = MYGET_PUSH_URL
		});
	});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("Package")
	.IsDependentOn("Publish");

Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
