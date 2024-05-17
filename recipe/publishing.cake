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
				var packagePath = BuildSettings.PackagingDirectory + packageName;
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
				var packagePath = BuildSettings.PackagingDirectory + packageName;
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
				var packagePath = BuildSettings.PackagingDirectory + packageName;
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

	public static void CreateDraftRelease(string releaseVersion)
	{
		if (CommandLineOptions.NoPush)
			_context.Information($"NoPush option skipping creation of draft release for version {releaseVersion}");
		else
		{
			string releaseName = $"{BuildSettings.Title} {releaseVersion}";

			_context.Information($"Creating draft release for {releaseName}");

			try
			{
				_context.GitReleaseManagerCreate(BuildSettings.GitHubAccessToken, BuildSettings.GitHubOwner, BuildSettings.GitHubRepository, new GitReleaseManagerCreateSettings()
				{
					Name = releaseName,
					Milestone = releaseVersion
				});
			}
			catch
			{
				_context.Error($"Unable to create draft release for {releaseName}.");
				_context.Error($"Check that there is a {releaseVersion} milestone with at least one closed issue.");
				_context.Error("");
				throw;
			}
		}
	}

	public static void DownloadDraftRelease()
	{
		if (!BuildSettings.IsReleaseBranch)
			throw new Exception("DownloadDraftRelease requires a release branch!");

		string milestone = BuildSettings.BranchName.Substring(8);

		_context.GitReleaseManagerExport(BuildSettings.GitHubAccessToken, BuildSettings.GitHubOwner, BuildSettings.GitHubRepository, "DraftRelease.md",
			new GitReleaseManagerExportSettings() { TagName = milestone });
	}

	public static void CreateProductionRelease()
	{
		if (!BuildSettings.IsProductionRelease)
		{
			_context.Information("Skipping CreateProductionRelease because this is not a production release");
		}
		else if (CommandLineOptions.NoPush)
			_context.Information($"NoPush option skipping creation of production release for version {BuildSettings.PackageVersion}");
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

			_context.Information($"Publishing release {tagName} to GitHub");
			_context.Information($"  Assets: {assets}");

			_context.GitReleaseManagerAddAssets(token, owner, repository, tagName, assets);
			//_context.GitReleaseManagerClose(token, owner, repository, tagName); Issue 67
		}
	}
}
