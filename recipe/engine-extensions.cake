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

	public PackageReference NuGetPackage => new PackageReference(NuGetId, Version);
	public PackageReference ChocoPackage => new PackageReference(ChocoId, Version);
	
	// Return an extension specifier using the same package ids as this
	// one but specifying a particular version to be used.
	public ExtensionSpecifier SetVersion(string version)
	{
		return new ExtensionSpecifier(NuGetId, ChocoId, version);
	}

	// Install this extension for a package
	public void InstallExtension(PackageDefinition targetPackage)
	{
		PackageReference extensionPackage = targetPackage.PackageType == PackageType.Chocolatey
			? ChocoPackage
			: NuGetPackage;
		
		extensionPackage.Install(targetPackage.ExtensionInstallDirectory);

		// Temporary fix when building engine to copy testcentric.engine.core
		// we just built into the pluggable agents we are using.	
		if (targetPackage.PackageId != "TestCentric.Engine")
			return;

		// TODO: Figure out how to break the circularity created by the fact that
		// the pluggable agents depend on the engine core while the engine project
		// tests require working copies of three pluggable agents. Ideally, this
		// code should be in the engine project itself but that's not possible so
		// long as all packaging steps are within the same task.

		var engineCoreBinDir = BuildSettings.SourceDirectory + "TestEngine/testcentric.engine.core/bin/Release/";
		var targetDir = targetPackage.ExtensionInstallDirectory + extensionPackage.Id + "." + extensionPackage.Version + "/tools/agent/";

		switch (extensionPackage.Id)
		{
			//case "NUnit.Extension.Net462PluggableAgent":
			//case "nunit-extension-net462-pluggable-agent":
			//	var sourceDir = engineCoreBinDir + "net462/";
			//	Console.WriteLine($"Copying {sourceDir}");
			//	Console.WriteLine($"     to {targetDir}");
			//	BuildSettings.Context.CopyDirectory(sourceDir, targetDir);
			//	break;
			case "NUnit.Extension.Net60PluggableAgent":
			case "nunit-extension-net60-pluggable-agent":
			case "NUnit.Extension.Net70PluggableAgent":
			case "nunit-extension-net70-pluggable-agent":
				var sourceDir = engineCoreBinDir + "netcoreapp3.1/";
				Console.WriteLine($"Copying {sourceDir}");
				Console.WriteLine($"     to {targetDir}");
				BuildSettings.Context.CopyDirectory(sourceDir, targetDir);
				break;
		}
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
