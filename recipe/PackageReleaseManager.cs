// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    /// <summary>
    /// Publishes packages to MyGet, NuGet and Chocolatey, and creates GitHub releases,
    /// following the publishing rules in BuildSettings.
    /// </summary>
    public static class PackageReleaseManager
    {
        private static ICakeContext _context;

        static PackageReleaseManager()
        {
            _context = BuildSettings.Context;
        }

        private static bool _hadErrors = false;

        public static void Publish()
        {
            _hadErrors = false;

            PublishToMyGet();
            PublishToNuGet();
            PublishToChocolatey();

            if (_hadErrors)
                throw new Exception("One of the publishing steps failed.");
        }

        public static void PublishToMyGet()
        {
            if (!BuildSettings.ShouldPublishToMyGet)
                _context.Information("Nothing to publish to MyGet from this run.");
            else if (CommandLineOptions.NoPush)
                _context.Information("NoPush option suppressing publication to MyGet");
            else
                foreach (var package in BuildSettings.Packages)
                {
                    var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
                    var packagePath = BuildSettings.PackageDirectory + packageName;
                    try
                    {
                        if (package.PackageType == PackageType.NuGet)
                            PushNuGetPackage(packagePath, BuildSettings.MyGetApiKey, BuildSettings.MyGetPushUrl);
                        else if (package.PackageType == PackageType.Chocolatey)
                            PushChocolateyPackage(packagePath, BuildSettings.MyGetApiKey, BuildSettings.MyGetPushUrl);
                    }
                    catch (Exception ex)
                    {
                        _context.Error(ex.Message);
                        _hadErrors = true;
                    }
                }
        }

        public static void PublishToNuGet()
        {
            if (!BuildSettings.ShouldPublishToNuGet)
                _context.Information("Nothing to publish to NuGet from this run.");
            else if (CommandLineOptions.NoPush)
                _context.Information("NoPush option suppressing publication to NuGet");
            else
                foreach (var package in BuildSettings.Packages)
                {
                    var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
                    var packagePath = BuildSettings.PackageDirectory + packageName;
                    try
                    {
                        if (package.PackageType == PackageType.NuGet)
                            PushNuGetPackage(packagePath, BuildSettings.NuGetApiKey, BuildSettings.NuGetPushUrl);
                    }
                    catch (Exception ex)
                    {
                        _context.Error(ex.Message);
                        _hadErrors = true;
                    }
                }
        }

        public static void PublishToChocolatey()
        {
            if (!BuildSettings.ShouldPublishToChocolatey)
                _context.Information("Nothing to publish to Chocolatey from this run.");
            else if (CommandLineOptions.NoPush)
                _context.Information("NoPush option suppressing publication to Chocolatey");
            else
                foreach (var package in BuildSettings.Packages)
                {
                    var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
                    var packagePath = BuildSettings.PackageDirectory + packageName;
                    try
                    {
                        if (package.PackageType == PackageType.Chocolatey)
                            PushChocolateyPackage(packagePath, BuildSettings.ChocolateyApiKey, BuildSettings.ChocolateyPushUrl);
                    }
                    catch (Exception ex)
                    {
                        _context.Error(ex.Message);
                        _hadErrors = true;
                    }
                }
        }

        private static void PushNuGetPackage(FilePath package, string apiKey, string url)
        {
            CheckPackageExists(package);
            _context.NuGetPush(package, new NuGetPushSettings() { ApiKey = apiKey, Source = url });
        }

        private static void PushChocolateyPackage(FilePath package, string apiKey, string url)
        {
            CheckPackageExists(package);
            _context.ChocolateyPush(package, new ChocolateyPushSettings() { ApiKey = apiKey, Source = url });
        }

        private static void CheckPackageExists(FilePath package)
        {
            if (!_context.FileExists(package))
                throw new InvalidOperationException(
                    $"Package not found: {package.GetFilename()}.\nCode may have changed since package was last built.");
        }

        private const string DRAFT_RELEASE_ERROR =
            "A direct call to CreateDraftRelease is permitted only:\r\n" +
            "  * Locally on main, with HEAD tagged with the release version (e.g. 2.0.0-beta9)\r\n" +
            "  * On a release branch (release-x.y.z)\r\n" +
            "  * Using option --packageVersion to specify a release version";

        /// <summary>
        /// The version CreateDraftRelease creates a draft release for, or null if it
        /// creates none. Setup uses this too, to require a GitHub token only when needed.
        /// </summary>
        public static string DraftReleaseVersion =>
            CommandLineOptions.PackageVersion.Exists
                ? CommandLineOptions.PackageVersion.Value
                : BuildSettings.IsReleaseBranch
                    ? BuildSettings.BranchName.Substring(8)
                    // On a production release, we only create a draft release if the target
                    // was called directly on a local system. Otherwise, we risk either an
                    // exception or possibly modifying a user-edited draft release.
                    : IsLocalDirectCall("CreateDraftRelease") && BuildSettings.IsProductionRelease
                        ? BuildSettings.PackageVersion : null;

        /// <summary>
        /// The version UpdateReleaseNotes updates the release notes for, or null if none.
        /// </summary>
        public static string ReleaseNotesVersion =>
            CommandLineOptions.PackageVersion.Exists
                ? CommandLineOptions.PackageVersion.Value
                : BuildSettings.IsProductionRelease
                    ? BuildSettings.PackageVersion : null;

        // Cake matches target names without regard to case, so we do the same.
        private static bool IsLocalDirectCall(string target) =>
            BuildSettings.IsLocalBuild &&
            string.Equals(CommandLineOptions.Target.Value, target, StringComparison.OrdinalIgnoreCase);

        public static void CreateDraftRelease()
        {
            string releaseVersion = DraftReleaseVersion;

            if (releaseVersion != null)
            {
                string releaseName = $"{BuildSettings.Title} {releaseVersion}";
                _context.Information($"Creating draft release for {releaseName}");

                try
                {
                    GRM.Create(releaseName, releaseVersion, IsPreReleaseVersion(releaseVersion));
                }
                catch
                {
                    _context.Error($"Unable to create draft release for {releaseName}.");
                    _context.Error($"Check that there is a {releaseVersion} milestone with at least one closed issue.");
                    _context.Error("");
                    throw;
                }
            }
            else
            {
                if (IsLocalDirectCall("CreateDraftRelease"))
                    throw new InvalidOperationException(DRAFT_RELEASE_ERROR);
                else
                    _context.Information("Skipping creation of draft release because this is not a release branch");
            }
        }

        private const string UPDATE_RELEASE_ERROR =
            "A direct call to UpdateReleaseNotes is permitted only:\r\n" +
            "  * On the main branch tagged for a production release\r\n" +
            "  * Using option --packageVersion to specify a release version";

        public static void UpdateReleaseNotes()
        {
            string releaseVersion = ReleaseNotesVersion;

            if (releaseVersion == null)
                throw new InvalidOperationException(UPDATE_RELEASE_ERROR);

            string releaseName = $"{BuildSettings.Title} {releaseVersion}";
            _context.Information($"Updating release notes for {releaseName}");

            try
            {
                GRM.Create(releaseName, releaseVersion, IsPreReleaseVersion(releaseVersion));
            }
            catch
            {
                _context.Error($"Unable to update release notes for {releaseName}.");
                _context.Error($"Check that there is a {releaseVersion} milestone with a matching release.");
                _context.Error("");
                throw;
            }

        }

        public static void DownloadDraftRelease()
        {
            if (!BuildSettings.IsReleaseBranch)
                throw new Exception("DownloadDraftRelease requires a release branch!");

            string milestone = BuildSettings.BranchName.Substring(8);

            GRM.Export("DraftRelease.md", milestone);
        }

        public static void CreateProductionRelease()
        {
            if (!BuildSettings.IsProductionRelease)
            {
                _context.Information("Skipping CreateProductionRelease because this is not a production release");
            }
            else
            {
                string token = BuildSettings.GitHubAccessToken;
                string owner = BuildSettings.GitHubOwner;
                string repository = BuildSettings.GitHubRepository;
                string tagName = BuildSettings.PackageVersion;
                string assets = string.Join<string>(',', BuildSettings.Packages.Select(p => p.PackageFilePath));

                _context.Information($"Publishing release {tagName} to GitHub");
                _context.Information($"  Assets: {assets}");

                GRM.AddAssets(tagName, assets);
                GRM.Close(tagName);
            }
        }

        // The version being released, not the version being built, decides this: a draft
        // release may be created from a build of some other version, e.g. on a release
        // branch or with --packageVersion. A SemVer version with a pre-release part,
        // such as 2.0.0-beta9, has a hyphen.
        private static bool IsPreReleaseVersion(string releaseVersion) => releaseVersion.Contains('-');

        private static class GRM
        {
            // With preRelease, GitHub shows the release as a pre-release when it is published.
            public static void Create(string name, string milestone, bool preRelease)
            {
                Execute($"create -n \"{name}\" -m {milestone}" + (preRelease ? " --pre" : ""));
            }

            public static void Export(string filename, string milestone)
            {
                Execute($"export -f \"{filename}\" -m {milestone}");
            }

            public static void AddAssets(string tagname, string assets)
            {
                Execute($"addasset -t {tagname} -a \"{assets}\"");
            }

            public static void Close(string milestone)
            {
                Execute($"close -m {milestone}");
            }

            private static void Execute(string arguments)
            {
                // The token is a secret, so anything we log uses RenderSafe(), which redacts it.
                // Cake's own logging of the process command line does the same.
                var builder = new ProcessArgumentBuilder()
                    .Append("gitreleasemanager")
                    .Append(arguments)
                    .AppendSwitchSecret("--token", " ", BuildSettings.GitHubAccessToken ?? "")
                    .AppendSwitch("-o", " ", BuildSettings.GitHubOwner)
                    .AppendSwitch("-r", " ", BuildSettings.GitHubRepository);

                if (CommandLineOptions.NoPush)
                    _context.Information($"NoPush option skipping command `{builder.RenderSafe()}`");
                else
                    _context.StartProcess("dotnet", new ProcessSettings { Arguments = builder });
            }
        }
    }
}
