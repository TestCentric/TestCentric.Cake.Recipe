///////////////////////////////////////////////////////// /////////////
// ZIP PACKAGE
//////////////////////////////////////////////////////////////////////

public class ZipPackage : PackageDefinition
{
    /// <summary>
    /// Construct passing all required arguments
    /// </summary>
    /// <param name="packageType">A PackageType value specifying one of the four known package types</param>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName</param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="testRunner">A TestRunner instance used to run package tests.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
    /// <param name="symbols">An array of PackageChecks to be made on the symbol package, if one is created. Optional. Only supported for nuget packages.</param>
    /// <param name="tests">An array of PackageTests to be run against the package. Optional.</param>
	public ZipPackage(
        string id, 
        string source = null, 
        string basePath = null, 
        TestRunner testRunner = null,
        PackageCheck[] checks = null, 
        IEnumerable<PackageTest> tests = null,
        PackageReference[] preloadedExtensions = null,
        ExtensionSpecifier[] bundledExtensions = null)
    : base (
        PackageType.Zip, 
        id, 
        source: source,
        basePath: basePath, 
        testRunner: testRunner, 
        checks: checks,
        tests: tests,
        preloadedExtensions: preloadedExtensions)
    {
        BundledExtensions = bundledExtensions;
    }

    public override string PackageFileName => $"{PackageId}-{PackageVersion}.zip";
    public override string PackageInstallDirectory => BuildSettings.ZipTestDirectory;
    public override string PackageResultDirectory => $"{BuildSettings.ZipResultDirectory}{PackageId}/";
    public override string ExtensionInstallDirectory => $"{BuildSettings.ZipTestDirectory}{PackageId}/bin/addins/";
  
    public ExtensionSpecifier[] BundledExtensions { get; }

    public override void BuildPackage()
    {
        if (string.IsNullOrEmpty(PackageSource))
            throw new ArgumentException(
                $"Package source must be specified fir a Zip package.", "source");
        else if (!PackageSource.EndsWith(".zspec"))
            throw new ArgumentException(
                $"Invalid package source specified: {PackageSource}. Must be of type '.zspec'.", "source");
        else if (!System.IO.File.Exists(PackageSource))
            throw new FileNotFoundException(
                $"Package source not found: {PackageSource}");

        // Get zip specification, which tells what to put in the zip
		var spec = new ZipSpecification(PackageSource);

        string baseDir = BuildSettings.OutputDirectory;
	    string zipImageDir = BuildSettings.ZipImageDirectory;
        _context.CreateDirectory(zipImageDir);
        _context.CleanDirectory(zipImageDir);

        // Follow the specification to create the zip image file
		foreach(var fileItem in spec.Files)
		{
            //Console.WriteLine(fileItem.ToString());

			var source = baseDir + fileItem.Source?.Trim();
			var target = zipImageDir + fileItem.Target?.Trim();

			_context.CreateDirectory(target);

			if (IsPattern(source))
				_context.CopyFiles(source, target, true);
			else
				_context.CopyFileToDirectory(source, target);
		}

        if (BundledExtensions != null)
            foreach(ExtensionSpecifier extensionSpecifier in BundledExtensions)
                extensionSpecifier.NuGetPackage.Install(zipImageDir + "bin/addins/");
                

        // Zip the directory to create package
        _context.Zip(BuildSettings.ZipImageDirectory, BuildSettings.PackageDirectory + PackageFileName);

		bool IsPattern(string s) => s.IndexOfAny(new [] {'*', '?' }) >0;
    }

    public override void InstallPackage()
    {
        _context.Unzip(BuildSettings.PackageDirectory + PackageFileName, PackageInstallDirectory + PackageId);
    }

    protected override bool IsRemovableExtensionDirectory(DirectoryPath dirPath)
    {
        var dirName = dirPath.GetDirectoryName();

        if (!dirName.StartsWith("NUnit.Extension."))
            return false;

        foreach (var extension in BundledExtensions)
            if (dirName.StartsWith(extension.NuGetId + "."))
                return false;

        return true;
    }

    class ZipSpecification
    {
        public List<ZipFileSpecification> Files = new List<ZipFileSpecification>();

	    public ZipSpecification(string fileName)
	    {
		    if (string.IsNullOrEmpty(fileName))
			    throw new ArgumentException("The fileName was not specified", "fileName");

		    foreach (string line in System.IO.File.ReadAllLines(fileName))
		    {
                string source = line;
                string target = null;

                if (string.IsNullOrWhiteSpace(line)) continue;
			    int hash = line.IndexOf('#');
                if (hash >= 0)
                {
                    source = line.Substring(0, hash);
                    if (string.IsNullOrWhiteSpace(source)) continue;
                }

			    int arrow = source.IndexOf("=>");			
			    if (arrow > 0)
                {
                    target = source.Substring(arrow + 2);
                    source = source.Substring(0,arrow);
                }

			    Files.Add(new ZipFileSpecification(source, target));
		    }
        }
    }

    class ZipFileSpecification
    {
	    public ZipFileSpecification(string source, string target = null)
	    {
		    Source = source;
		    Target = target;
	    }

	    public string Source;
	    public string Target;

        public override string ToString() => $"{Source} => {Target}";
    }

}
