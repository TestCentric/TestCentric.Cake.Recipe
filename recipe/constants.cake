// This file contains both real constants and static readonly variables used
// as constants. All values are initialized before any instance variables.

// URLs for uploading packages
private const string MYGET_PUSH_URL = "https://www.myget.org/F/testcentric/api/v2";
private const string NUGET_PUSH_URL = "https://api.nuget.org/v3/index.json";
private const string CHOCO_PUSH_URL = "https://push.chocolatey.org/";

// Environment Variable names holding API keys
private const string TESTCENTRIC_MYGET_API_KEY = "TESTCENTRIC_MYGET_API_KEY";
private const string TESTCENTRIC_NUGET_API_KEY = "TESTCENTRIC_NUGET_API_KEY";
private const string TESTCENTRIC_CHOCO_API_KEY = "TESTCENTRIC_CHOCO_API_KEY";
private const string GITHUB_ACCESS_TOKEN = "GITHUB_ACCESS_TOKEN";
// Older names used for fallback
private const string MYGET_API_KEY = "MYGET_API_KEY";
private const string NUGET_API_KEY = "NUGET_API_KEY";
private const string CHOCO_API_KEY = "CHOCO_API_KEY";

// Pre-release labels that we publish
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };

// Defaults
const string DEFAULT_CONFIGURATION = "Release";
private static readonly string[] DEFAULT_VALID_CONFIGS = { "Release", "Debug" };

const string DEFAULT_COPYRIGHT = "Copyright (c) Charlie Poole and TestCentric contributors.";
static readonly string[] DEFAULT_STANDARD_HEADER = new[] {
	"// ***********************************************************************",
	$"// {DEFAULT_COPYRIGHT}",
	"// Licensed under the MIT License. See LICENSE file in root directory.",
	"// ***********************************************************************"
};

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

// Standard values for TestCentric packages
static readonly string[] TESTCENTRIC_AUTHORS = new[] { "Charlie Poole" };
static readonly string[] TESTCENTRIC_OWNERS = new[] { "Charlie Poole" };
const string TESTCENTRIC_COPYRIGHT = "Copyright (c) 2021-2023 Charlie Poole";
static readonly Uri TESTCENTRIC_PROJECT_URL = new Uri("https://test-centric.org/recipe");
static readonly NuSpecLicense TESTCENTRIC_LICENSE = new NuSpecLicense() { Type = "expression", Value="MIT" };
static readonly Uri TESTCENTRIC_ICON_URL = new Uri("https://test-centric.org/assets/img/testcentric_128x128.png");
static readonly string TESTCENTRIC_ICON = "testcentric.png";
