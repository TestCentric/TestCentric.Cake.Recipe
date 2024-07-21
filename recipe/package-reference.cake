// Representation of a package reference, containing everything needed to install it
public class PackageReference
{
	private ICakeContext _context;

	public string Id { get; }
	public string Version { get; }

	// Static members provide reference to known packages
	public static PackageReference Engine = new PackageReference(
		"TestCentric.Engine", "2.0.0-beta3");
	public static PackageReference EngineCore = new PackageReference(
		"TestCentric.Engine.Core", "2.0.0-beta3");
	public static PackageReference EngineApi = new PackageReference(
		"TestCentric.Engine.Api", "2.0.0-beata3");
	public static PackageReference AgentCore = new PackageReference(
		"TestCentric.Agent.Core", "2.0.0");
	public static PackageReference Extensibility = new PackageReference(
		"TestCentric.Extensibility", "2.0.0");
	public static PackageReference ExtensibilityApi = new PackageReference(
		"TestCentric.ExtensibilityApi", "2.0.0");
	public static PackageReference Metadata = new PackageReference(
		"TestCentric.Metadata", "2.0.0");
	public static PackageReference GuiRunner = new PackageReference(
		"TestCentric.GuiRunner", "2.0.0-beta3-1");
	public static PackageReference InternalTrace = new PackageReference(
		"TestCentric.InternalTrace", "1.0.0");

	public PackageReference(string id, string version)
	{
		_context = BuildSettings.Context;

		Id = id;
		Version = version;
	}

    public PackageReference LatestDevBuild => GetLatestDevBuild();
	public PackageReference LatestRelease => GetLatestRelease();
	
	private PackageReference GetLatestDevBuild()
	{
		var packageList = _context.NuGetList(Id, new NuGetListSettings()
		{
			Prerelease = true, 
			Source = new [] { $"https://www.myget.org/F/{GITHUB_OWNER}/api/v3/index.json" } 
		} );

		foreach (var package in packageList)
			return new PackageReference(package.Name, package.Version);
		
		return this;
	}

	private PackageReference GetLatestRelease()
	{
		var packageList = _context.NuGetList(Id, new NuGetListSettings()
		{
			Prerelease = true, 
			Source = new [] { 
				"https://www.nuget.org/api/v2/",
				"https://community.chocolatey.org/api/v2/" } 
		} );

		// TODO: There seems to be an error in NuGet or in Cake, causing the list to
		// contain ALL TestCentric packages, so we check the Id in this loop.
		foreach (var package in packageList)
			if (package.Name == Id)
				return new PackageReference(Id, package.Version);

		return this;
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

			var packageSources = new []
			{
				BuildSettings.LocalPackagesDirectory,
				"https://www.myget.org/F/testcentric/api/v3/index.json",
				"https://api.nuget.org/v3/index.json",
				"https://community.chocolatey.org/api/v2/"
			};

			Console.WriteLine("Package Sources:");
			foreach(var source in packageSources)
				Console.WriteLine($"  {source}");
			Console.WriteLine();

			_context.NuGetInstall(Id,
				new NuGetInstallSettings()
				{
					OutputDirectory = installDirectory,
					Version = Version,
					Source = packageSources
				});
		}
	}
}
