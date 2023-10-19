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
	static public bool NoBuild;
	static public bool NoPush;

	public static void Initialize(ICakeContext context)
	{
		// The name of the TARGET task to be run, e.g. Test.
		Target = GetArgument("target", 1, "Default");

		// The name of the configuration to build, test and/or package, e.g. Debug.
		Configuration = GetArgument("configuration", 1, DEFAULT_CONFIGURATION);
		
		// If used, specifies the full package version, including any pre-release
		// suffix. Otherwise we use GitVersion to calculate the package version.
		PackageVersion = GetArgument<string>("packageVersion", 4, null);
		
		// Specifies the level of package testing, which is normally set
		// automatically for different types of builds like CI, PR, etc.
		// If not used, level is are calculated in BuildSettings.
		TestLevel = GetArgument("testLevel", 4, GetArgument("level", 1, 0));

		// Default TraceLevel for package tests
		TraceLevel = GetArgument("trace", 2, "Off");

		// If true, no builds are done. If any build target is used,
		// a message is displayed.
		NoBuild = HasArgument("nobuild", 3);

		// If true, no publishing or releasing will be done. If any
		// publish or release targets are used, a message is displayed.
		NoPush = HasArgument("nopush", 3);

		T GetArgument<T>(string name, int minLength, T defaultValue)
		{
			for (int len = name.Length; len >= minLength; len--)
			{
				string abbrev = name.Substring(0,len);
				if (context.HasArgument(abbrev))
					return context.Argument<T>(abbrev);
			}
			
			return defaultValue;
		}

		bool HasArgument(string name, int minLength)
		{
			for (int len = name.Length; len >= minLength; len--)
				if (context.HasArgument(name.Substring(0,len)))
					return true;

			return false;
		}
	}
}