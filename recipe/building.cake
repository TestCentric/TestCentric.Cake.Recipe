// This file contains all tasks related to building the project

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
		Information("Cleaning " + BuildSettings.OutputDirectory);
		CleanDirectory(BuildSettings.OutputDirectory);

        Information("Cleaning Package Directory");
        CleanDirectory(BuildSettings.PackageDirectory);
	});

//////////////////////////////////////////////////////////////////////
// CLEAN AND DELETE ALL OBJ DIRECTORIES
//////////////////////////////////////////////////////////////////////

Task("CleanAll")
	.Description("Clean both configs and all obj directories")
	.Does(() =>
	{
		Information("Cleaning all output directories");
		CleanDirectory(BuildSettings.ProjectDirectory + "bin/");

        Information("Cleaning Package Directory");
        CleanDirectory(BuildSettings.PackageDirectory);

		Information("Deleting object directories");
		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});


//////////////////////////////////////////////////////////////////////
// DELETE ALL OBJ DIRECTORIES
//////////////////////////////////////////////////////////////////////

Task("DeleteObjectDirectories")
	.Does(() =>
	{
		Information("Deleting object directories");
		foreach (var dir in GetDirectories("src/**/obj/"))
			DeleteDirectory(dir, new DeleteDirectorySettings() { Recursive = true });
	});

//////////////////////////////////////////////////////////////////////
// RESTORE NUGET PACKAGES
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
	.Does(() =>
	{
		NuGetRestore(BuildSettings.SolutionFile, BuildSettings.RestoreSettings);
	});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("NuGetRestore")
	.IsDependentOn("CheckHeaders")
	.Does(() =>
	{
		if (BuildSettings.SolutionFile == null)
			throw new Exception("Unable to perform Build. No solution file was provided.");

		MSBuild(BuildSettings.SolutionFile, BuildSettings.MSBuildSettings
			.WithProperty("Version", BuildSettings.PackageVersion));
	});
