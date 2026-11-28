// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    /// <summary>
    /// Defines a NuGet package that contains a Cake recipe.
    /// </summary>
    public class RecipePackage : NuGetPackage
    {
        private IEnumerable<FilePath> _recipeFiles;
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

            if (!_hasNuSpec)
            {
                if (content == null) content = "recipe/*.cs";
                _recipeFiles = _context.GetFiles(content).Select(f => f.GetFilename());

                PackageChecks = new PackageCheck[] {
                    HasFiles("LICENSE.txt", "README.md", "testcentric.png"),
                    HasDirectory(RecipeFilesTarget).WithFiles(_recipeFiles.ToArray())
                };
            }

            PackageChecks = (PackageChecks ?? new PackageCheck[0])
                .Append(new RecipeVersionCheck($"{RecipeFilesTarget}/{RECIPE_FILE_NAME}", PackageVersion))
                .ToArray();
        }

        // The committed recipe/Recipe.cs sets RECIPE_VERSION to a placeholder. The package
        // gets a copy with the package version instead, written to the package directory,
        // so building never changes a tracked file. A nuspec must pack this copy in place of
        // recipe/Recipe.cs: <file src="packages/generated/Recipe.cs" ... />
        private const string RECIPE_FILE_NAME = "Recipe.cs";
        private static string VersionedRecipeFile => BuildSettings.PackageDirectory + "generated/" + RECIPE_FILE_NAME;

        public override void BuildPackage()
        {
            // Written here rather than in the constructor: the package directory
            // is cleaned before packaging, and other targets need no copy.
            SIO.Directory.CreateDirectory(SIO.Path.GetDirectoryName(VersionedRecipeFile));
            SIO.File.WriteAllText(VersionedRecipeFile, $$"""
                {{string.Join(Environment.NewLine, DEFAULT_STANDARD_HEADER)}}

                public static partial class Program
                {
                    public const string RECIPE_VERSION = "{{PackageVersion}}";
                }

                """);

            base.BuildPackage();
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
                    foreach (FilePath filePath in _recipeFiles)
                        settings.Files.Add(new NuSpecContent() {
                            Source = filePath.GetFilename().ToString() == RECIPE_FILE_NAME ? VersionedRecipeFile : $"recipe/{filePath}",
                            Target = RecipeFilesTarget });
                    // Compile the recipe files into the build script that references the package.
                    // Addin dependencies need a nuspec; this path doesn't add them.
                    settings.ContentFiles = new[] { new NuSpecContentFile { Include = "cs/any/**/*.cs", BuildAction = "Compile" } };
                }

                return settings;
            }
        }

        // Where the recipe's .cs files go in the package, so that #:package compiles them
        private string RecipeFilesTarget => $"contentFiles/cs/any/{PackageId}";

        /// <summary>
        /// Checks that the packed Recipe.cs sets RECIPE_VERSION to the package version.
        /// </summary>
        private class RecipeVersionCheck : PackageCheck
        {
            private string _relFilePath;
            private string _version;

            public RecipeVersionCheck(string relFilePath, string version)
            {
                _relFilePath = relFilePath;
                _version = version;
            }

            public override bool ApplyTo(DirectoryPath testDirPath)
            {
                var filePath = testDirPath.CombineWithFilePath(_relFilePath);
                if (!CheckFileExists(filePath))
                    return false;

                string expected = $"RECIPE_VERSION = \"{_version}\"";
                if (!SIO.File.ReadAllText(filePath.FullPath).Contains(expected))
                    return DisplayError($"{_relFilePath} does not contain {expected}.");

                return true;
            }
        }
    }
}
