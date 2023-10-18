// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

public static class CommandLineOptions
{
	static public string Target;
	static public string Configuration;
	static public string PackageVersion;
	static public int TestLevel;
	static public string TraceLevel;
	static public bool NoPush;

	public static void Initialize(ICakeContext context)
	{
		// The name of the TARGET task to be run, e.g. Test.
		Target = GetArgument("target|t", "Default");

		// The name of the configuration to build, test and/or package, e.g. Debug.
		Configuration = GetArgument("configuration|c", DEFAULT_CONFIGURATION);
		
		// Specifies the full package version, including any pre-release
		// suffix. This version is used directly instead of the default
		// version from the script or that calculated by GitVersion.
		// Note that all other versions (AssemblyVersion, etc.) are
		// derived from the package version.
		PackageVersion = GetArgument<string>("packageVersion|package", null);
		
		// Specifies the level of package testing, which is normally set
		// automatically for different types of builds like CI, PR, etc.
		// Used by developers to test packages locally without creating
		// a PR or publishing the package. Defined levels are
		//	1. Normal CI tests run every time you build a package
		//  2. Adds more tests for PRs and Dev builds uploaded to MyGet
		//  3. Adds even more tests prior to publishing a release
		TestLevel = GetArgument("testLevel|level", 0);

		// Default TraceLevel for package tests
		TraceLevel = GetArgument("trace", "Off");
		
		// If true, no publishing or releasing will be done. If any
		// publish or release targets are used, a message is displayed.
		NoPush = context.HasArgument("nopush");

		T GetArgument<T>(string pattern, T defaultValue)
		{
			foreach (var name in pattern.Split('|'))
				if (context.HasArgument(name))
					return context.Argument<T>(name);
			
			return defaultValue;
		}
	}
}