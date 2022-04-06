using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.OctoVersion;

namespace Octopus.NukeBuildComponents
{
    public interface IOctopusNukeBuild : INukeBuild
    {
        string TargetPackageDescription { get; }

        Enumeration Config { get; }

        [Solution] Solution? Solution => TryGetValue(() => Solution);

        [OctoVersion] OctoVersionInfo? OctoVersionInfo => TryGetValue(() => OctoVersionInfo);

        AbsolutePath SourceDirectory => RootDirectory / "source";
        public AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
        public AbsolutePath PublishDirectory => RootDirectory / "publish";
        public AbsolutePath LocalPackagesDir => RootDirectory / ".." / "LocalPackages";
    }
}