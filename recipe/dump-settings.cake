// DUMP SETTINGS
//////////////////////////////////////////////////////////////////////

Task("DumpSettings")
	.Does<BuildParameters>((parameters) =>
	{
		Console.WriteLine("\nTASKS");
		Console.WriteLine("Target:                       " + parameters.Target);
		Console.WriteLine("TasksToExecute:               " + string.Join(", ", parameters.TasksToExecute));

		Console.WriteLine("\nENVIRONMENT");
		Console.WriteLine("IsLocalBuild:                 " + parameters.IsLocalBuild);
		Console.WriteLine("IsRunningOnWindows:           " + parameters.IsRunningOnWindows);
		Console.WriteLine("IsRunningOnUnix:              " + parameters.IsRunningOnUnix);
		Console.WriteLine("IsRunningOnAppVeyor:          " + parameters.IsRunningOnAppVeyor);

		Console.WriteLine("\nVERSIONING");
		Console.WriteLine("PackageVersion:               " + parameters.PackageVersion);
		Console.WriteLine("AssemblyVersion:              " + parameters.AssemblyVersion);
		Console.WriteLine("AssemblyFileVersion:          " + parameters.AssemblyFileVersion);
		Console.WriteLine("AssemblyInformationalVersion: " + parameters.AssemblyInformationalVersion);
		Console.WriteLine("SemVer:                       " + parameters.BuildVersion.SemVer);
		Console.WriteLine("IsPreRelease:                 " + parameters.BuildVersion.IsPreRelease);
		Console.WriteLine("PreReleaseLabel:              " + parameters.BuildVersion.PreReleaseLabel);
		Console.WriteLine("PreReleaseSuffix:             " + parameters.BuildVersion.PreReleaseSuffix);

		Console.WriteLine("\nDIRECTORIES");
		Console.WriteLine("Project:   " + parameters.ProjectDirectory);
		Console.WriteLine("Output:    " + parameters.OutputDirectory);
		Console.WriteLine("Source:    " + parameters.SourceDirectory);
		Console.WriteLine("NuGet:     " + parameters.NuGetDirectory);
		Console.WriteLine("Choco:     " + parameters.ChocoDirectory);
		Console.WriteLine("Package:   " + parameters.PackageDirectory);
		Console.WriteLine("ZipImage:  " + parameters.ZipImageDirectory);
		Console.WriteLine("ZipTest:   " + parameters.ZipTestDirectory);
		Console.WriteLine("NuGetTest: " + parameters.NuGetTestDirectory);
		Console.WriteLine("ChocoTest: " + parameters.ChocolateyTestDirectory);

		Console.WriteLine("\nBUILD");
		Console.WriteLine("Configuration:   " + parameters.Configuration);
		//Console.WriteLine("Engine Runtimes: " + string.Join(", ", parameters.SupportedEngineRuntimes));

		Console.WriteLine("\nPACKAGING");
		Console.WriteLine("MyGetPushUrl:              " + parameters.MyGetPushUrl);
		Console.WriteLine("NuGetPushUrl:              " + parameters.NuGetPushUrl);
		Console.WriteLine("ChocolateyPushUrl:         " + parameters.ChocolateyPushUrl);
		Console.WriteLine("MyGetApiKey:               " + (!string.IsNullOrEmpty(parameters.MyGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("NuGetApiKey:               " + (!string.IsNullOrEmpty(parameters.NuGetApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));
		Console.WriteLine("ChocolateyApiKey:          " + (!string.IsNullOrEmpty(parameters.ChocolateyApiKey) ? "AVAILABLE" : "NOT AVAILABLE"));

		Console.WriteLine("\nPUBLISHING");
		Console.WriteLine("ShouldPublishToMyGet:      " + parameters.ShouldPublishToMyGet);
		Console.WriteLine("ShouldPublishToNuGet:      " + parameters.ShouldPublishToNuGet);
		Console.WriteLine("ShouldPublishToChocolatey: " + parameters.ShouldPublishToChocolatey);

		Console.WriteLine("\nRELEASING");
		Console.WriteLine("BranchName:                   " + parameters.BranchName);
		Console.WriteLine("IsReleaseBranch:              " + parameters.IsReleaseBranch);
		Console.WriteLine("IsProductionRelease:          " + parameters.IsProductionRelease);
	});
