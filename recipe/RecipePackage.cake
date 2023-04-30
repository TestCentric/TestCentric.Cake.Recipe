public class RecipePackage : NuGetPackage
{
    const string RECIPE_DIR = "content";

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
        RecipeContent = content ?? _context.GetFiles($"{RECIPE_DIR}/*.cake").ToArray();

        PackageChecks = new PackageCheck[] {
		    HasFiles("LICENSE.txt", "testcentric.png"),
            HasDirectory(RECIPE_DIR).WithFiles(RecipeContent)
        };
    }

    public FilePath[] RecipeContent { get; }
}