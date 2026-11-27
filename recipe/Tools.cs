// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

// Usings needed throughout the recipe. Cake script applied these to every
// loaded file; in C# they must be global to do the same.
global using Cake.Common.Build;
global using Cake.Git;
global using Cake.MinVer;
global using System.Xml;
global using SIO = System.IO;

// Addins used by the recipe are package references in the build script:
//   #:package Cake.Git@5.0.1
//   #:package Cake.MinVer@4.0.0

public static partial class Program
{
    /// <summary>
    /// Install all tools used by the recipe. Called by BuildSettings.Initialize
    /// before anything that needs them, such as calculating the version.
    /// </summary>
    static void InstallRecipeTools()
    {
        InstallTool("nuget:?package=NuGet.CommandLine&version=7.0.1");
        InstallTool("dotnet:?package=minver-cli&version=8.0.0");
        InstallTool("dotnet:?package=GitReleaseManager.Tool&version=0.20.0");
    }
}
