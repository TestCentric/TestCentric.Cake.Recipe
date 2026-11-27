// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

// Load all tools used by the recipe
#tool NuGet.CommandLine&version=7.0.1
#tool dotnet:?package=minver-cli&version=8.0.0
#tool dotnet:?package=GitReleaseManager.Tool&version=0.20.0
#addin nuget:?package=Cake.Git&version=5.0.1
#addin nuget:?package=Cake.MinVer&version=4.0.0

using Cake.Git;
using Cake.MinVer;