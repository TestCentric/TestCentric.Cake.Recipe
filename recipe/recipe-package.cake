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
        string id,
        string basePath = null,
        string content = "./recipe/*.cake",
        string title = null,
        string summary = null,
        string description = null,
        string[] releaseNotes = null,
        string[] tags = null)
    : base (
        id, 
        basePath: basePath ?? BuildSettings.ProjectDirectory,
        title: title ?? id.Replace(".", " "),
        summary: summary ?? "No summary provided.",
        description: description ?? "No description provided.",
        releaseNotes: releaseNotes ?? new [] { "No release notes provided." },
        tags: tags)
    {
        _cakeFiles = _context.GetFiles(content).Select(f => f.GetFilename());

        PackageChecks = new PackageCheck[] {
		    HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
            HasDirectory("content").WithFiles(_cakeFiles.ToArray())
        };
    }

    protected override NuGetPackSettings NuGetPackSettings
    {
        get
        {
            var settings = base.NuGetPackSettings;

            settings.Files.Add(new NuSpecContent() { Source="LICENSE.txt" });
            settings.Files.Add(new NuSpecContent() { Source="README.md" });
            settings.Files.Add(new NuSpecContent() { Source="testcentric.png" });
            foreach (FilePath filePath in _cakeFiles)
                settings.Files.Add(new NuSpecContent() { Source=$"recipe/{filePath}", Target="content" });

            return settings;
        }
    }
}