using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.ValueInjection;

namespace Octopus.NukeBuildComponents
{
    public interface IOctopusNukeBuild : INukeBuild
    {
        Enumeration Config { get; }

        [Solution] Solution Solution => ValueInjectionUtility.TryGetValue(() => Solution);

        [OctoVersion] OctoVersionInfo OctoVersionInfo => ValueInjectionUtility.TryGetValue(() => OctoVersionInfo);

        AbsolutePath SourceDirectory => RootDirectory / "source";
        public AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
        public AbsolutePath PublishDirectory => RootDirectory / "publish";
        public AbsolutePath LocalPackagesDir => RootDirectory / ".." / "LocalPackages";
    }
}