// Representation of a package, containing everything needed to install it
public class PackageSpecifier
{
	private ICakeContext _context;

	public string Id { get; }
	public string Version { get; }

	public PackageSpecifier(string id, string version)
	{
		_context = BuildSettings.Context;

		Id = id;
		Version = version;
	}

	public bool IsInstalled(string installDirectory)
	{
		return _context.GetDirectories($"{installDirectory}{Id}.*").Count > 0;
	}

	public void Install(string installDirectory)
	{
		if (!IsInstalled(installDirectory))
		{
			Banner.Display($"Installing {Id} version {Version}");

			_context.NuGetInstall(Id,
				new NuGetInstallSettings()
				{
					OutputDirectory = installDirectory,
					Version = Version
				});
		}
	}

}

// Representation of an extension, for use by PackageTests. Because our
// extensions usually exist as both nuget and chocolatey packages, each
// extension may have a nuget id, a chocolatey id or both. A default version
// is used unless the user overrides it.
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

	public PackageSpecifier NuGetPackage => new PackageSpecifier(NuGetId, Version);
	public PackageSpecifier ChocoPackage => new PackageSpecifier(ChocoId, Version);
	
	// Return an extension specifier using the same package ids as this
	// one but specifying a particular version to be used.
	public ExtensionSpecifier SetVersion(string version)
	{
		return new ExtensionSpecifier(NuGetId, ChocoId, version);
	}

	// Install this extension for a package
	public void InstallExtension(PackageDefinition package)
	{
		if (package.PackageType == PackageType.Chocolatey)
			ChocoPackage.Install(package.ExtensionInstallDirectory);
		else
			NuGetPackage.Install(package.ExtensionInstallDirectory);
	}
}

// Static Variables representing well-known Extensions
public static ExtensionSpecifier NUnitV2Driver = new ExtensionSpecifier(
	"NUnit.Extension.NUnitV2Driver", "nunit-extension-nunit-v2-driver", "3.9.0");
public static ExtensionSpecifier NUnitProjectLoader = new ExtensionSpecifier(
	"NUnit.Extension.NUnitProjectLoader", "nunit-extension-nunit-project-loader", "3.7.1");
public static ExtensionSpecifier Net20PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net20PluggableAgent", "nunit-extension-net20-pluggable-agent", "2.0.0");
public static ExtensionSpecifier Net462PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net462PluggableAgent", "nunit-extension-net462-pluggable-agent", "2.0.1");
public static ExtensionSpecifier NetCore21PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.NetCore21PluggableAgent", "nunit-extension-netcore21-pluggable-agent", "2.1.0");
public static ExtensionSpecifier NetCore31PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.NetCore31PluggableAgent", "nunit-extension-netcore31-pluggable-agent", "2.0.0");
public static ExtensionSpecifier Net50PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net50PluggableAgent", "nunit-extension-net50-pluggable-agent", "2.0.0");
public static ExtensionSpecifier Net60PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net60PluggableAgent", "nunit-extension-net60-pluggable-agent", "2.0.0");
public static ExtensionSpecifier Net70PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net70PluggableAgent", "nunit-extension-net70-pluggable-agent", "2.0.0");
public static ExtensionSpecifier Net80PluggableAgent = new ExtensionSpecifier(
	"NUnit.Extension.Net80PluggableAgent", "nunit-extension-net80-pluggable-agent", "2.1.0");


// Static class holding information about known extensions. A default
// set of extensions are provided and may be accessed via properties.
// Users may define additional extensions as needed.
public static class EngineExtensions
{
	public static Dictionary<string, ExtensionSpecifier> _extensions;

	static EngineExtensions()
	{
		_extensions = new Dictionary<string,ExtensionSpecifier>();

		// Add well-known extensions to the dictionary
		Define(nameof(NUnitV2Driver), NUnitV2Driver);
		Define(nameof(NUnitProjectLoader), NUnitProjectLoader);
		Define(nameof(Net20PluggableAgent), Net20PluggableAgent);
		Define(nameof(Net462PluggableAgent), Net462PluggableAgent);
		Define(nameof(NetCore21PluggableAgent), NetCore21PluggableAgent);
		Define(nameof(NetCore31PluggableAgent), NetCore31PluggableAgent);
		Define(nameof(Net50PluggableAgent), Net50PluggableAgent);
		Define(nameof(Net60PluggableAgent), Net60PluggableAgent);
		Define(nameof(Net70PluggableAgent), Net70PluggableAgent);
		Define(nameof(Net80PluggableAgent), Net80PluggableAgent);
	}

	// Retrieve an extension specifier by name
	public static ExtensionSpecifier ByName(string name)
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
