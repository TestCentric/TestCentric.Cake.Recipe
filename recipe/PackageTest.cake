// Representation of a single test to be run against a pre-built package.
// Each test has a Level, with the following values defined...
//  0 Do not run - used for temporarily disabling a test
//  1 Run for all CI tests - that is every time we test packages
//  2 Run only on PRs, dev builds and when publishing
//  3 Run only when publishing
public struct PackageTest
{
	public int Level;
	public string Name;
	public string Description;
	public string Arguments;
	public ExpectedResult ExpectedResult;
	public ExtensionSpecifier[] ExtensionsNeeded;
	
	public PackageTest(int level, string name, string description, string arguments, ExpectedResult expectedResult, params ExtensionSpecifier[] extensionsNeeded)
	{
        if (name == null)
            throw new ArgumentNullException(nameof(name));
		if (description == null)
			throw new ArgumentNullException(nameof(description));
		if (arguments == null)
			throw new ArgumentNullException(nameof(arguments));
		if (expectedResult == null)
			throw new ArgumentNullException(nameof(expectedResult));

		Level = level;
		Name = name;
		Description = description;
		Arguments = arguments;
		ExpectedResult = expectedResult;
		ExtensionsNeeded = extensionsNeeded;
	}
}

// Representation of an extension, for use by PackageTests. Each extension
// may have a nuget id, a chocolatey id or both. A default version is used
// unless the user overrides it.
public class ExtensionSpecifier
{
	public ExtensionSpecifier(string nugetId, string chocoId, string version)
	{
		NuGetId = nugetId;
		ChocoId = chocoId;
		Version = version;
	}

	public string NuGetId { get; }
	public string ChocoId { get; }
	public string Version { get; }

	
	// Return an extension specifier using the same package ids as this
	// one but specifying a particular version to be used.
	public ExtensionSpecifier SetVersion(string version)
	{
		return new ExtensionSpecifier(NuGetId, ChocoId, version);
	}
}

// Static class holding information about known extensions. A default
// set of extensions are provided and may be accessed via properties.
// Users may define additional extensions as needed.
public static class EngineExtensions
{
	public static Dictionary<string, ExtensionSpecifier> _extensions;

	static EngineExtensions()
	{
		_extensions = new Dictionary<string,ExtensionSpecifier>();

		// Define well-known extensions - these should match the built-in extension properties
		Define("NUnitV2Driver", new ExtensionSpecifier("NUnit.Extension.NUnitV2Driver", "nunit-extension-nunit-v2-driver", "3.9.0"));
		Define("NUnitProjectLoader", new ExtensionSpecifier("NUnit.Extension.NUnitProjectLoader", "nunit-extension-nunit-project-loader", "3.7.1"));
		Define("Net20PluggableAgent", new ExtensionSpecifier("NUnit.Extension.Net20PluggableAgent", "nunit-extension-net20-pluggable-agent", "2.0.0"));
		Define("NetCore21PluggableAgent", new ExtensionSpecifier("NUnit.Extension.NetCore21PluggableAgent", "nunit-extension-netcore21-pluggable-agent", "2.1.0"));
		Define("Net80PluggableAgent", new ExtensionSpecifier("NUnit.Extension.Net80PluggableAgent", "nunit-extension-net80-pluggable-agent", "2.1.0"));
	}

	// Built-In Extension Properties
	public static ExtensionSpecifier NUnitV2Driver => Extension(nameof(NUnitV2Driver));
	public static ExtensionSpecifier NUnitProjectLoader => Extension(nameof(NUnitProjectLoader));
	public static ExtensionSpecifier Net20PluggableAgent => Extension(nameof(Net20PluggableAgent));
	public static ExtensionSpecifier NetCore21PluggableAgent => Extension(nameof(NetCore21PluggableAgent));
	public static ExtensionSpecifier Net80PluggableAgent => Extension(nameof(Net80PluggableAgent));

	// Retrieve an extension specifier by name
	public static ExtensionSpecifier Extension(string name)
	{
		if (!_extensions.ContainsKey(name))
			throw new ArgumentException($"Extension '{name}' is not registered", nameof(name));

		return _extensions[name];
	}

	// Define a known extension
	public static void Define(string name, ExtensionSpecifier extension)
	{
		_extensions[name] = extension;
	}
}
