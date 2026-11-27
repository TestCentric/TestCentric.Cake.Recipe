// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

public static partial class Program
{
    /// <summary>
    /// Versions are calculated by MinVer from the most recent tag reachable from HEAD.
    /// For example, after tag 2.0.0-beta.1, the next commit builds as 2.0.0-beta.1.1
    /// and the one after as 2.0.0-beta.1.2, so every build gets a new, higher version.
    /// After a final release such as 2.0.0, builds are 2.0.1-alpha.0.1, 2.0.1-alpha.0.2, ...
    /// which sort below any 2.0.1-alpha.1 tagged later. A build exactly on a tag uses the tag itself.
    /// </summary>
    public class BuildVersion
    {
        public BuildVersion(ICakeContext context)
        {
            if (context==null)
                throw new ArgumentNullException(nameof(context));

            // If a specific version is requested on the command line, use it. Otherwise get it from MinVer.
            string packageVersion = CommandLineOptions.PackageVersion.Value ?? context.MinVer().Version;

            int dash = packageVersion.IndexOf('-');
            IsPreRelease = dash > 0;

            string versionPart = packageVersion;
            string suffix = "";
            string label = "";

            if (IsPreRelease)
            {
                versionPart = packageVersion.Substring(0, dash);
                suffix = packageVersion.Substring(dash + 1);
                foreach (char c in suffix)
                {
                    if (!char.IsLetter(c))
                        break;
                    label += c;
                }
            }

            Version version = new Version(versionPart);
            SemVer = version.ToString(3);
            PreReleaseLabel = label;
            PreReleaseSuffix = suffix;

            PackageVersion = packageVersion;
            AssemblyVersion = SemVer + ".0";
            AssemblyFileVersion = SemVer;
            AssemblyInformationalVersion = packageVersion;
        }

        public string PackageVersion { get; }
        public string AssemblyVersion { get; }
        public string AssemblyFileVersion { get; }
        public string AssemblyInformationalVersion { get; }

        public string SemVer { get; }
        public bool IsPreRelease { get; }
        public string PreReleaseLabel { get; }
        public string PreReleaseSuffix { get; }

        // A fractional pre-release has more than one dot in its suffix, like
        // beta.1.3 or alpha.0.2. These are untagged builds, which we never
        // publish anywhere but MyGet.
        public bool IsFractionalPreRelease => PreReleaseSuffix.Count(c => c == '.') > 1;
    }
}
