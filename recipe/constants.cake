// This file contains both real constants and static readonly variables used
// as constants. All values are initialized before any instance variables.

// Alias used throughout the recipe
using SIO = System.IO;

// GitHub owner is the TestCentric organization
const string GITHUB_OWNER = "TestCentric";

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

// Standardized project directory structure - not changeable by user
const string SRC_DIR			= "src/";
const string BIN_DIR			= "bin/";
const string NUGET_DIR			= "nuget/";
const string CHOCO_DIR			= "choco/";
const string ZIP_DIR			= "zip/";
const string PACKAGE_DIR		= "package/";
const string PKG_TEST_DIR		= "package/tests/";
const string NUGET_TEST_DIR		= "package/tests/nuget/";
const string NUGET_RUNNER_DIR	= "package/tests/nuget/runners/";
const string CHOCO_TEST_DIR		= "package/tests/choco/";
const string CHOCO_RUNNER_DIR	= "package/tests/choco/runners/";
const string ZIP_TEST_DIR		= "package/tests/zip/";
const string PKG_RSLT_DIR		= "package/results/";
const string NUGET_RSLT_DIR		= "package/results/nuget/";
const string CHOCO_RSLT_DIR		= "package/results/choco/";
const string ZIP_RSLT_DIR		= "package/results/zip/";
const string ZIP_IMG_DIR		= "package/zipimage/";
const string TOOLS_DIR			= "tools/";

const string LOCAL_PACKAGES_DIR	= "../LocalPackages";
private static readonly string[] LABELS_WE_ADD_TO_LOCAL_FEED = { "dev", "alpha", "beta", "rc" };

// WARNING: When comparing versions, it's important to keep in mind some anomalies.
// For example, new Version(6,0) is NOT equal to new Version(6,0,0). Instead the
// former is less than the latter. Bugs can creep in easily. To avoid this, use
// the following manifest static variables rather than using new each time.
// Additional values should be defined as needed.
static readonly Version V_1_1	= new Version(1,1);
static readonly Version V_2_0	= new Version(2,0);
static readonly Version V_2_1	= new Version(2,1);
static readonly Version V_3_1	= new Version(3,1);
static readonly Version V_3_5	= new Version(3,5);
static readonly Version V_4_6_2	= new Version(4,6,2);
static readonly Version V_5_0	= new Version(5,0);
static readonly Version V_6_0	= new Version(6,0);
static readonly Version V_7_0	= new Version(7,0);
static readonly Version V_8_0	= new Version(8,0);

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
private static readonly string[] LABELS_WE_PUBLISH_ON_MYGET = { "dev", "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_NUGET = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_PUBLISH_ON_CHOCOLATEY = { "alpha", "beta", "rc" };
private static readonly string[] LABELS_WE_RELEASE_ON_GITHUB = { "alpha", "beta", "rc" };

// Common values used in all TestCentric packages
static readonly string[] TESTCENTRIC_PACKAGE_AUTHORS = new[] { "Charlie Poole" };
static readonly string[] TESTCENTRIC_PACKAGE_OWNERS = new[] { "Charlie Poole" };
static readonly NuSpecLicense TESTCENTRIC_LICENSE = new NuSpecLicense() { Type = "expression", Value="MIT" };

const string TESTCENTRIC_ICON = "testcentric.png";
// TODO: Automatic update of year
const string TESTCENTRIC_COPYRIGHT = "Copyright (c) 2021-2024 Charlie Poole";
const string TESTCENTRIC_PROJECT_URL = "https://test-centric.org/";
const string TESTCENTRIC_GITHUB_URL = "https://github.com/TestCentric/";
const string TESTCENTRIC_RAW_URL = "https://raw.githubusercontent.com/TestCentric/";
const string TESTCENTRIC_ICON_URL = "https://test-centric.org/assets/img/testcentric_128x128.png";
const string TESTCENTRIC_MAILING_LIST_URL = "https://groups.google.com/forum/#!forum/nunit-discuss";
