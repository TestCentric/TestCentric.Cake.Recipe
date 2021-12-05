//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

static readonly string[] PACKAGE_SOURCES =
{
   "https://www.nuget.org/api/v2",
   "https://www.myget.org/F/nunit/api/v2",
   "https://www.myget.org/F/testcentric/api/v2"
};

Task("NuGetRestore")
	.Does<BuildSettings>((settings) =>
	{
		NuGetRestore(settings.SolutionFile, new NuGetRestoreSettings()
		{
			Source = PACKAGE_SOURCES,
			Verbosity = NuGetVerbosity.Detailed
		});
	});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("NuGetRestore")
	.IsDependentOn("CheckHeaders")
	.Does<BuildSettings>((settings) =>
	{
		if (IsRunningOnWindows())
		{
			MSBuild(settings.SolutionFile, new MSBuildSettings()
				.SetConfiguration(settings.Configuration)
				.SetMSBuildPlatform(MSBuildPlatform.Automatic)
				.SetVerbosity(Verbosity.Minimal)
				.SetNodeReuse(false)
				.SetPlatformTarget(PlatformTarget.MSIL)
			);
		}
		else
		{
			XBuild(settings.SolutionFile, new XBuildSettings()
				.WithTarget("Build")
				.WithProperty("Configuration", settings.Configuration)
				.SetVerbosity(Verbosity.Minimal)
			);
		}
	});
