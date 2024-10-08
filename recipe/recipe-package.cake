public class RecipePackage : NuGetPackage
{
    private IEnumerable<FilePath> _cakeFiles;
    private bool _hasNuSpec = false;

    /// <summary>
    /// Construct passing all required arguments
    /// </summary>
    /// <param name="id">A string containing the package ID, used as the root of the PackageName.</param>
    /// <param name="basePath">Path used in locating binaries for the package.</param>
    /// <param name="checks">An array of PackageChecks be made on the content of the package. Optional.</param>
	public RecipePackage(
        string id,
        string basePath = null,
        string source = null,
        string content = null,
        string title = null,
        string description = null,
        string summary = null,
        string[] releaseNotes = null,
        string[] tags = null ,
        PackageCheck[] checks = null
    )
    : base (
        id, 
        basePath: basePath ?? BuildSettings.ProjectDirectory,
        source: source,
        summary: summary ?? "No summary provided.",
        description: description ?? "No description provided.",
        releaseNotes: releaseNotes ?? new [] { "No release notes provided." },
        tags: tags ?? new [] { "testcentric" },
        checks: checks
    )
    {
        if (source != null && source.EndsWith(".nuspec"))
            _hasNuSpec = true;

        // Create or rewrite recipe.cake so it has the version we are building
        using (var writer = new StreamWriter("./recipe/recipe.cake"))
            writer.Write($$"""
                public static class Recipe
                {
                    public static string Version => "{{PackageVersion}}";
                }
                """);

        if (!_hasNuSpec)
        {
            if (content == null) content = "recipe/*.cake";
            _cakeFiles = _context.GetFiles(content).Select(f => f.GetFilename());

            PackageChecks = new PackageCheck[] {
                HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
                HasDirectory("content").WithFiles(_cakeFiles.ToArray())
            };
        }
    }

    protected override NuGetPackSettings NuGetPackSettings
    {
        get
        {
            var settings = base.NuGetPackSettings;

            if (!_hasNuSpec)
            {
                settings.Files.Add(new NuSpecContent() { Source = "LICENSE.txt" });
                settings.Files.Add(new NuSpecContent() { Source = "README.md" });
                settings.Files.Add(new NuSpecContent() { Source = "testcentric.png" });
                foreach (FilePath filePath in _cakeFiles)
                    settings.Files.Add(new NuSpecContent() { Source = $"recipe/{filePath}", Target = "content" });
            }

            return settings;
        }
    }
}