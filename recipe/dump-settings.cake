// DUMP SETTINGS
//////////////////////////////////////////////////////////////////////

Task("DumpSettings")
	.Does<BuildSettings>((settings) =>
	{
		Console.WriteLine("\nTASKS");
		Console.WriteLine("Target:                       " + settings.Target);
		Console.WriteLine("TasksToExecute:               " + string.Join(", ", settings.TasksToExecute));

		Console.WriteLine("\nENVIRONMENT");
		Console.WriteLine("IsLocalBuild:                 " + settings.IsLocalBuild);
		Console.WriteLine("IsRunningOnWindows:           " + settings.IsRunningOnWindows);
		Console.WriteLine("IsRunningOnUnix:              " + settings.IsRunningOnUnix);
		Console.WriteLine("IsRunningOnAppVeyor:          " + settings.IsRunningOnAppVeyor);

		Console.WriteLine("\nVERSIONING");
		Console.WriteLine("PackageVersion:               " + settings.PackageVersion);
		Console.WriteLine("AssemblyVersion:              " + settings.AssemblyVersion);
		Console.WriteLine("AssemblyFileVersion:          " + settings.AssemblyFileVersion);
		Console.WriteLine("AssemblyInformationalVersion: " + settings.AssemblyInformationalVersion);
		Console.WriteLine("SemVer:                       " + settings.BuildVersion.SemVer);
		Console.WriteLine("IsPreRelease:                 " + settings.BuildVersion.IsPreRelease);
		Console.WriteLine("PreReleaseLabel:              " + settings.BuildVersion.PreReleaseLabel);
		Console.WriteLine("PreReleaseSuffix:             " + settings.BuildVersion.PreReleaseSuffix);

		Console.WriteLine("\nDIRECTORIES");
		Console.WriteLine("Project:   " + settings.ProjectDirectory);
		Console.WriteLine("Output:    " + settings.OutputDirectory);
		Console.WriteLine("Source:    " + settings.SourceDirectory);
		Console.WriteLine("NuGet:     " + settings.NuGetDirectory);
		Console.WriteLine("Choco:     " + settings.ChocoDirectory);
		Console.WriteLine("Package:   " + settings.PackageDirectory);
		Console.WriteLine("ZipImage:  " + settings.ZipImageDirectory);
		Console.WriteLine("ZipTest:   " + settings.ZipTestDirectory);
		Console.WriteLine("NuGetTest: " + settings.NuGetTestDirectory);
		Console.WriteLine("ChocoTest: " + settings.ChocolateyTestDirectory);

		Console.WriteLine("\nBUILD");
		Console.WriteLine("Configuration:   " + settings.Configuration);
		//Console.WriteLine("Engine Runtimes: " + string.Join(", ", settings.SupportedEngineRuntimes));

		Console.WriteLine("\nPACKAGING");
		Console.WriteLine("MyGetPushUrl:              " + settings.MyGetPushUrl);
		Console.WriteLine("NuGetPushUrl:              " + settings.NuGetPushUrl);
		Console.WriteLine("ChocolateyPushUrl:         " + settings.ChocolateyPushUrl);
		Console.WriteLine("MyGetApiKey:               " + (!string.IsNullOrEmpty(settings.MyGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("NuGetApiKey:               " + (!string.IsNullOrEmpty(settings.NuGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("ChocolateyApiKey:          " + (!string.IsNullOrEmpty(settings.ChocolateyApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));

		Console.WriteLine("\nPUBLISHING");
		Console.WriteLine("ShouldPublishToMyGet:      " + settings.ShouldPublishToMyGet);
		Console.WriteLine("ShouldPublishToNuGet:      " + settings.ShouldPublishToNuGet);
		Console.WriteLine("ShouldPublishToChocolatey: " + settings.ShouldPublishToChocolatey);

		Console.WriteLine("\nRELEASING");
		Console.WriteLine("BranchName:                   " + settings.BranchName);
		Console.WriteLine("IsReleaseBranch:              " + settings.IsReleaseBranch);
		Console.WriteLine("IsProductionRelease:          " + settings.IsProductionRelease);
	});
