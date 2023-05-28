// This file contains both real constants and static readonly variables used
// as constants. All values are initialized before any instance variables.

// WARNING: When comparing versions, it's important to keep in mind some anomalies.
// For example, new Version(6,0) is NOT equal to new Version(6,0,0). Instead the
// former is less than the latter. Bugs can creep in easily. To avoid this, use
// the following manifest static variables rather than using new each time.
// Additional values should be defined as needed.
static readonly Version V_1_1 = new Version(1,1);
static readonly Version V_2_0 = new Version(2,0);
static readonly Version V_2_1 = new Version(2,1);
static readonly Version V_3_1 = new Version(3,1);
static readonly Version V_3_5 = new Version(3,5);
static readonly Version V_4_6_2 = new Version(4,6,2);
static readonly Version V_5_0 = new Version(5,0);
static readonly Version V_6_0 = new Version(6,0);
static readonly Version V_7_0 = new Version(7,0);
static readonly Version V_8_0 = new Version(8,0);

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

// Common values used in all TestCentric packages
static readonly string[] TESTCENTRIC_PACKAGE_AUTHORS = new[] { "Charlie Poole" };
static readonly string[] TESTCENTRIC_PACKAGE_OWNERS = new[] { "Charlie Poole" };
static readonly NuSpecLicense TESTCENTRIC_LICENSE = new NuSpecLicense() { Type = "expression", Value="MIT" };

const string TESTCENTRIC_ICON = "testcentric.png";
const string TESTCENTRIC_COPYRIGHT = "Copyright (c) 2021-2023 Charlie Poole";
const string TESTCENTRIC_PROJECT_URL = "https://test-centric.org/";
const string TESTCENTRIC_GITHUB_URL = "https://github.com/TestCentric/";
const string TESTCENTRIC_ICON_URL = "https://github.com/TestCentric/assets/img/testcentric_128x128.png";

static readonly string PROJECT_REPOSITORY_URL = TESTCENTRIC_GITHUB_URL + BuildSettings.GitHubRepository + "/";

static readonly string TESTCENTRIC_LICENSE_URL = "https://raw.githubusercontent.com/TestCentric/" + BuildSettings.GitHubRepository + "/main/LICENSE.txt";
const string TESTCENTRIC_MAILING_LIST_URL = "https://groups.google.com/forum/#!forum/nunit-discuss";
