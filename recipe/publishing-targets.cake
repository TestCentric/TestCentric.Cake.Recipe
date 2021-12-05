//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGES
//////////////////////////////////////////////////////////////////////

static bool hadPublishingErrors = false;

Task("Publish")
	.Description("Publish nuget and chocolatey packages according to the current settings")
	// Temporarily removed until all targets are added
	//.IsDependentOn("Package")
	.IsDependentOn("PublishToMyGet")
	.IsDependentOn("PublishToNuGet")
	.IsDependentOn("PublishToChocolatey")
	.Does(() =>
	{
		if (hadPublishingErrors)
			throw new Exception("One of the publishing steps failed.");
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToMyGet")
	.Description("Publish packages to MyGet")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.IsProductionRelease && !settings.IsDevelopmentRelease)
			Information("Nothing to publish to MyGet from this run.");
		else
			try
			{
				PushNuGetPackage(settings.NuGetPackage, settings.MyGetApiKey, settings.MyGetPushUrl);
				PushChocolateyPackage(settings.ChocolateyPackage, settings.MyGetApiKey, settings.MyGetPushUrl);
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				hadPublishingErrors = true;
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToNuGet")
	.Description("Publish packages to NuGet")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.IsProductionRelease)
			Information("Nothing to publish to NuGet from this run.");
		else
			try
			{
				PushNuGetPackage(settings.NuGetPackage, settings.NuGetApiKey, settings.NuGetPushUrl);
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				hadPublishingErrors = true;
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToChocolatey")
	.Description("Publish packages to Chocolatey")
	.Does<BuildSettings>((settings) =>
	{
		if (!settings.IsProductionRelease)
			Information("Nothing to publish to Chocolatey from this run.");
		else
			try
			{
				PushChocolateyPackage(settings.ChocolateyPackage, settings.ChocolateyApiKey, settings.ChocolateyPushUrl);
			}
			catch (Exception ex)
			{
				Error(ex.Message);
				hadPublishingErrors = true;
			}
	});

private void PushNuGetPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	NuGetPush(package, new NuGetPushSettings() { ApiKey = apiKey, Source = url });
}

private void PushChocolateyPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	ChocolateyPush(package, new ChocolateyPushSettings() { ApiKey = apiKey, Source = url });
}

private void CheckPackageExists(FilePath package)
{
	if (!FileExists(package))
		throw new InvalidOperationException(
			$"Package not found: {package.GetFilename()}.\nCode may have changed since package was last built.");
}
