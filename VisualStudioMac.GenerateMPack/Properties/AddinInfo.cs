using Mono.Addins;
using Mono.Addins.Description;
using System.Runtime.Versioning;
using System.Reflection;

[assembly: AddinDependency("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly: AddinDependency("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]

[assembly: SupportedOSPlatform("macos10.15")]