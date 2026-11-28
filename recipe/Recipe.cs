// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    // "source" means the recipe was compiled from a checkout rather than from a
    // package. When the recipe package is built, RecipePackage packs a copy of
    // this file with the package version in its place; this file is not changed.
    public const string RECIPE_VERSION = "source";
}
