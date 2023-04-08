//////////////////////////////////////////////////////////////////////
// CREATE A DRAFT RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateDraftRelease")
	.Does(() =>
	{
		if (!BuildSettings.BuildVersion.IsReleaseBranch)
		{
			Information("Skipping Release creation because this is not a release branch");
		}
		else if (BuildSettings.NoPush)
			Information($"NoPush option skipping creation of draft release for version {BuildSettings.PackageVersion}");
		else
		{
			// NOTE: Since this is a release branch, the pre-release label
			// is "pre", which we don't want to use for the draft release.
			// The branch name contains the full information to be used
			// for both the name of the draft release and the milestone,
			// i.e. release-2.0.0, release-2.0.0-beta2, etc.
			string milestone = BuildSettings.BranchName.Substring(8);
			string releaseName = $"{BuildSettings.Title} {milestone}";

			Information($"Creating draft release for {releaseName}");

			try
			{
				GitReleaseManagerCreate(BuildSettings.GitHubAccessToken, BuildSettings.GitHubOwner, BuildSettings.GitHubRepository, new GitReleaseManagerCreateSettings()
				{
					Name = releaseName,
					Milestone = milestone
				});
			}
			catch
			{
				Error($"Unable to create draft release for {releaseName}.");
				Error($"Check that there is a {milestone} milestone with at least one closed issue.");
				Error("");
				throw;
			}
		}
	});

//////////////////////////////////////////////////////////////////////
// DOWNLOAD THE DRAFT RELEASE
//////////////////////////////////////////////////////////////////////

Task("DownloadDraftRelease")
	.Description("Download draft release for local use")
	.Does(() =>
	{
		if (!BuildSettings.IsReleaseBranch)
			throw new Exception("DownloadDraftRelease requires a release branch!");

		string milestone = BuildSettings.BranchName.Substring(8);

		GitReleaseManagerExport(BuildSettings.GitHubAccessToken, BuildSettings.GitHubOwner, BuildSettings.GitHubRepository, "DraftRelease.md",
			new GitReleaseManagerExportSettings() { TagName = milestone });
	});

//////////////////////////////////////////////////////////////////////
// CREATE A PRODUCTION RELEASE
//////////////////////////////////////////////////////////////////////

Task("CreateProductionRelease")
	.Does(() =>
	{
		if (!BuildSettings.IsProductionRelease)
		{
			Information("Skipping CreateProductionRelease because this is not a production release");
		}
		else if (BuildSettings.NoPush)
			Information($"NoPush option skipping creation of production release for version {BuildSettings.PackageVersion}");
		else
		{
			string token = BuildSettings.GitHubAccessToken;
			string owner = BuildSettings.GitHubOwner;
			string repository = BuildSettings.GitHubRepository;
			string tagName = BuildSettings.PackageVersion;
            string assets = string.Join<string>(',', BuildSettings.Packages.Select(p => p.PackageFilePath));

			//IsRunningOnWindows()
            //	? $"\"{BuildSettings.NuGetPackage},{BuildSettings.ChocolateyPackage}\""
            //	: $"\"{BuildSettings.NuGetPackage}\"";

			Information($"Publishing release {tagName} to GitHub");
			Information($"  Assets: {assets}");

			GitReleaseManagerAddAssets(token, owner, repository, tagName, assets);
			GitReleaseManagerClose(token, owner, repository, tagName);
		}
	});
