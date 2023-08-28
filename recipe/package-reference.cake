// Representation of a package reference, containing everything needed to install it
public class PackageReference
{
	private ICakeContext _context;

	public string Id { get; }
	public string Version { get; }

	public PackageReference(string id, string version)
	{
		_context = BuildSettings.Context;

		Id = id;
		Version = version;
	}

	public bool IsInstalled(string installDirectory)
	{
		return _context.GetDirectories($"{installDirectory}{Id}.*").Count > 0;
	}

	public void InstallExtension(PackageDefinition targetPackage)
	{
		Install(targetPackage.ExtensionInstallDirectory);
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
