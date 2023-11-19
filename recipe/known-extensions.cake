// Static class holding information about known extensions.
public static class KnownExtensions
{
	// Static Variables representing well-known Extensions
	public static ExtensionSpecifier NUnitV2Driver = new ExtensionSpecifier(
		"NUnit.Extension.NUnitV2Driver", "nunit-extension-nunit-v2-driver", "3.9.0");
	public static ExtensionSpecifier NUnitProjectLoader = new ExtensionSpecifier(
		"NUnit.Extension.NUnitProjectLoader", "nunit-extension-nunit-project-loader", "3.7.1");
	public static ExtensionSpecifier Net20PluggableAgent = new ExtensionSpecifier(
		"NUnit.Extension.Net20PluggableAgent", "nunit-extension-net20-pluggable-agent", "2.0.0");
	public static ExtensionSpecifier Net462PluggableAgent = new ExtensionSpecifier(
		"TestCentric.Extension.Net462PluggableAgent", "testcentric-extension-net462-pluggable-agent", "2.4.0");
	public static ExtensionSpecifier NetCore21PluggableAgent = new ExtensionSpecifier(
		"NUnit.Extension.NetCore21PluggableAgent", "nunit-extension-netcore21-pluggable-agent", "2.1.0");
	public static ExtensionSpecifier NetCore31PluggableAgent = new ExtensionSpecifier(
		"NUnit.Extension.NetCore31PluggableAgent", "nunit-extension-netcore31-pluggable-agent", "2.0.0");
	public static ExtensionSpecifier Net50PluggableAgent = new ExtensionSpecifier(
		"NUnit.Extension.Net50PluggableAgent", "nunit-extension-net50-pluggable-agent", "2.0.0");
	public static ExtensionSpecifier Net60PluggableAgent = new ExtensionSpecifier(
		"TestCentric.Extension.Net60PluggableAgent", "testcentric-extension-net60-pluggable-agent", "2.4.0");
	public static ExtensionSpecifier Net70PluggableAgent = new ExtensionSpecifier(
		"TestCentric.Extension.Net70PluggableAgent", "testcentric-extension-net70-pluggable-agent", "2.4.0");
	public static ExtensionSpecifier Net80PluggableAgent = new ExtensionSpecifier(
		"TestCentric.Extension.Net80PluggableAgent", "testcentric-extension-net80-pluggable-agent", "2.3.0");
}

// Representation of an extension, for use by PackageTests. Because our
// extensions usually exist as both nuget and chocolatey packages, each
// extension may have a nuget id, a chocolatey id or both. A default version
// is used unless the user overrides it using SetVersion.
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
	}
}
