abstract public class PackageDefinition
{
	protected BuildSettings _settings;
	public ICakeContext _context;

	public PackageDefinition(BuildSettings settings, string packageId, string packageSource)
	{
		_settings = settings;
		_context = settings.Context;

		PackageId = packageId;
		PackageSource = packageSource;
	}

	public string PackageId { get; }
	public string PackageSource { get; }

	public PackageCheck[] PackageChecks { get; set; } = new PackageCheck[0];
	public PackageTest[] PackageTests { get; set; } = new PackageTest[0];

	public string PackageTestDirectory { get; protected set; }

	public string PackageDirectory => _settings.PackageDirectory;
	public string PackageVersion => _settings.PackageVersion;
	public string PackageName => $"{PackageId}.{PackageVersion}.nupkg";

	public abstract void BuildPackage();
	public abstract void TestPackage();
}

public class NuGetPackage : PackageDefinition
{
	public NuGetPackage(BuildSettings settings, string packageId, string packageSource)
		: base(settings, packageId, packageSource) 
	{
		PackageTestDirectory = settings.NuGetTestDirectory;
	}

	public override void BuildPackage()
	{
		_context.CreateDirectory(PackageDirectory);

		_context.NuGetPack(PackageSource, new NuGetPackSettings()
		{
			Version = PackageVersion,
			OutputDirectory = PackageDirectory,
			NoPackageAnalysis = true
		});
	}

	public override void TestPackage()
	{
		new NuGetPackageTester(_settings, this).RunAllTests();
	}
}

public class ChocolateyPackage : PackageDefinition
{
	public ChocolateyPackage(BuildSettings settings, string packageId, string packageSource)
		: base(settings, packageId, packageSource)
	{
		PackageTestDirectory = settings.ChocolateyTestDirectory;
	}

	public override void BuildPackage()
	{
		_context.CreateDirectory(PackageDirectory);

		_context.ChocolateyPack(PackageSource, new ChocolateyPackSettings()
		{
			Version = PackageVersion,
			OutputDirectory = PackageDirectory
		});
	}

	public override void TestPackage()
	{
		new ChocolateyPackageTester(_settings, this).RunAllTests();
	}
}
