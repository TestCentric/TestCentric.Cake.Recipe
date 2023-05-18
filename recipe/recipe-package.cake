public class RecipePackage : NuGetPackage
{
    private IEnumerable<FilePath> _cakeFiles;

    /// <summary>
    /// Construct passing all required arguments
    /// </summary>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName</param>
    /// <param name="source">A string representing the source used to create the package, e.g. a nuspec file</param>
    /// <param name="basePath">Path used in locating binaries for the package</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
	public RecipePackage(
        string id, string source, string basePath, FilePath[] content = null)
      : base (id, source, basePath)
    {
        _cakeFiles = content ?? _context.GetFiles($"./recipe/*.cake").Select(f => f.GetFilename());

        PackageChecks = new PackageCheck[] {
		    HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
            HasDirectory("content").WithFiles(_cakeFiles.ToArray())
        };
    }

    public override void BuildPackage()
    {
        Console.WriteLine("Override called");

        var files = new List<NuSpecContent>();
        files.Add(new NuSpecContent() { Source="LICENSE.txt" });
        files.Add(new NuSpecContent() { Source="README.md" });
        files.Add(new NuSpecContent() { Source="testcentric.png" });
        foreach (FilePath filePath in _cakeFiles)
            files.Add(new NuSpecContent() { Source=$"recipe/{filePath}", Target="content" });

        var settings = DefaultPackSettings();
        settings.Title = "TestCentric Cake Recipe";
        settings.Description = "Cake Recipe used for building TestCentric applications and extensions";
        settings.Repository = new NuGetRepository() { Type="Git", Url="https://github.com/TestCentric/TestCentric.Cake.Recipe" };
        settings.Tags = new [] { "testcentric", "cake", "recipe" };
        //settings.ReleaseNotes = new [] { "" };
        settings.Files = files;

        _context.NuGetPack(settings);
    }
}