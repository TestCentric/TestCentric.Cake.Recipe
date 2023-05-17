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
        string id, string source, string basePath, TestRunner testRunner = null,
        PackageCheck[] checks = null, IEnumerable<PackageTest> tests = null,
        ExtensionSpecifier[] preloadedExtensions = null)
      : base (PackageType.Zip, id, source, basePath, testRunner: testRunner, 
        checks: checks, tests: tests, preloadedExtensions: preloadedExtensions)
    {
    }

    public override string PackageFileName => $"{PackageId}-{PackageVersion}.zip";
    public override string PackageInstallDirectory => BuildSettings.ZipTestDirectory;
    public override string PackageResultDirectory => $"{BuildSettings.ZipResultDirectory}{PackageId}/";
    public override string ExtensionInstallDirectory => $"{BuildSettings.ZipTestDirectory}{PackageId}/bin/addins/";
  
    public override void BuildPackage()
    {
        // Get zip specification, which tells what to put in the zip
		var spec = new ZipSpecification(PackageSource);

	    string zipImageDir = BuildSettings.ZipImageDirectory;
        _context.CreateDirectory(zipImageDir);
        _context.CleanDirectory(zipImageDir);

        // Follow the specification to create the zip image file
		foreach(var fileItem in spec.Files)
		{
            //Console.WriteLine(fileItem.ToString());

			var source = BasePath + fileItem.Source?.Trim();
			var target = zipImageDir + fileItem.Target?.Trim();

			_context.CreateDirectory(target);

			if (IsPattern(source))
				_context.CopyFiles(source, target, true);
			else
				_context.CopyFileToDirectory(source, target);
		}

        // Zip the directory to create package
        _context.Zip(BuildSettings.ZipImageDirectory, BuildSettings.PackageDirectory + PackageFileName);

		bool IsPattern(string s) => s.IndexOfAny(new [] {'*', '?' }) >0;
    }

    public override void InstallPackage()
    {
        _context.Unzip(BuildSettings.PackageDirectory + PackageFileName, PackageInstallDirectory + PackageId);
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
