using System.Reflection;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#endif
#if RELEASE
[assembly: AssemblyConfiguration ("RELEASE")]
#endif

[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Locale Names")]
[assembly: AssemblyCopyright("")]

[assembly: AssemblyVersion(ThisAssembly.Git.BaseVersion.Major
            + "."
            + ThisAssembly.Git.BaseVersion.Minor
            + "."
            + ThisAssembly.Git.BaseVersion.Patch
            + "."
            + ThisAssembly.Git.Commits)]

[assembly: AssemblyFileVersion(ThisAssembly.Git.BaseVersion.Major
            + "."
            + ThisAssembly.Git.BaseVersion.Minor
            + "."
            + ThisAssembly.Git.BaseVersion.Patch
            + "."
            + ThisAssembly.Git.Commits)]

[assembly: AssemblyInformationalVersion(ThisAssembly.Git.BaseVersion.Major
            + "."
            + ThisAssembly.Git.BaseVersion.Minor
            + "."
            + ThisAssembly.Git.BaseVersion.Patch
            + "."
            + ThisAssembly.Git.Commits
            + ThisAssembly.Git.SemVer.DashLabel
            + "-"
            + ThisAssembly.Git.Commit)]
