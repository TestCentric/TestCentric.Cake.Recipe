//////////////////////////////////////////////////////////////////////
// PACKAGING TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
	.IsDependentOn("Build")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

Task("PackageExistingBuild")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildVerifyAndTest();
	});

Task("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.BuildPackage();
	});

Task("InstallPackages")
	.IsDependentOn("BuildPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.InstallPackage();
	});

Task("VerifyPackages")
	.IsDependentOn("InstallPackages")
	.Does(() => {
		foreach(var package in BuildSettings.Packages)
			package.VerifyPackage();
	});
