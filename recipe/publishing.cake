//////////////////////////////////////////////////////////////////////
// PUBLISH PACKAGES
//////////////////////////////////////////////////////////////////////

static bool hadPublishingErrors = false;

Task("Publish")
	.Description("Publish nuget and chocolatey packages according to the current settings")
	.IsDependentOn("Package")
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
	.Does(() =>
	{
		if (!BuildSettings.ShouldPublishToMyGet)
			Information("Nothing to publish to MyGet from this run.");
		else if (BuildSettings.NoPush)
			Information("NoPush option suppressing publication to MyGet");
		else
			foreach (var package in BuildSettings.Packages)
			{
				var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
				var packagePath = BuildSettings.PackageDirectory + packageName;
				try
				{
					if (package.IsNuGetPackage)
						PushNuGetPackage(packagePath, BuildSettings.MyGetApiKey, BuildSettings.MyGetPushUrl);
					else if (package.IsChocolateyPackage)
						PushChocolateyPackage(packagePath, BuildSettings.MyGetApiKey, BuildSettings.MyGetPushUrl);
				}
				catch (Exception ex)
				{
					Error(ex.Message);
					hadPublishingErrors = true;
				}
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToNuGet")
	.Description("Publish packages to NuGet")
	.Does(() =>
	{
		if (!BuildSettings.ShouldPublishToNuGet)
			Information("Nothing to publish to NuGet from this run.");
		else if (BuildSettings.NoPush)
			Information("NoPush option suppressing publication to NuGet");
		else
			foreach (var package in BuildSettings.Packages)
			{
				var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
				var packagePath = BuildSettings.PackageDirectory + packageName;
				try
				{
					if (package.IsNuGetPackage)
						PushNuGetPackage(packagePath, BuildSettings.NuGetApiKey, BuildSettings.NuGetPushUrl);
				}
				catch (Exception ex)
				{
					Error(ex.Message);
					hadPublishingErrors = true;
				}
			}
	});

// This task may either be run by the PublishPackages task,
// which depends on it, or directly when recovering from errors.
Task("PublishToChocolatey")
	.Description("Publish packages to Chocolatey")
	.Does(() =>
	{
		if (!BuildSettings.ShouldPublishToChocolatey)
			Information("Nothing to publish to Chocolatey from this run.");
		else if (BuildSettings.NoPush)
			Information("NoPush option suppressing publication to NuGet");
		else
			foreach (var package in BuildSettings.Packages)
			{
				var packageName = $"{package.PackageId}.{BuildSettings.PackageVersion}.nupkg";
				var packagePath = BuildSettings.PackageDirectory + packageName;
				try
				{
					if (package.IsChocolateyPackage)
						PushChocolateyPackage(packagePath, BuildSettings.ChocolateyApiKey, BuildSettings.ChocolateyPushUrl);
				}
				catch (Exception ex)
				{
					Error(ex.Message);
					hadPublishingErrors = true;
				}
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
