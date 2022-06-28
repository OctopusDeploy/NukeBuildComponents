using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities;

namespace Octopus.NukeBuildComponents
{
    public interface IOctopusNukeBuild : INukeBuild
    {
        private static readonly Dictionary<MemberInfo, object> SValueCache = new();
        string TargetPackageDescription { get; }

        Enumeration Config { get; }

        [Solution] Solution Solution => TryGetValue(() => Solution)!;

        [Parameter("Branch name for OctoVersion to use to calculate the version number."
            , Name = "OCTOVERSION_CurrentBranch")]
        string BranchName => TryGetValue(() => BranchName)!;

        [Parameter("Whether to auto-detect the branch name - this is okay for a local " +
                   "build, but should not be used under CI.")]
        bool AutoDetectBranch => IsLocalBuild;

        [Required]
        [OctoVersionThatWorksWithBuildComponentsAttribute(AutoDetectBranchParameter = nameof(AutoDetectBranch),
            BranchParameter = nameof(BranchName),
            UpdateBuildNumber = true,
            Framework = "net6.0")]
        OctoVersionInfo OctoVersionInfo => TryGetInjectedOctoVersionInfo(() => OctoVersionInfo);

        AbsolutePath SourceDirectory => RootDirectory / "source";
        public AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
        public AbsolutePath PublishDirectory => RootDirectory / "publish";
        public AbsolutePath LocalPackagesDir => RootDirectory / ".." / "LocalPackages";

        // we need this custom method instead of the "normal" TryGetValue,
        // as we need to pass `instance: this` down
        OctoVersionInfo TryGetInjectedOctoVersionInfo(Expression<Func<OctoVersionInfo>> parameterExpression)
        {
            var parameter = parameterExpression.GetMemberInfo();

            OctoVersionInfo GetValue()
            {
                var attribute = parameter.GetCustomAttribute<OctoVersionThatWorksWithBuildComponentsAttribute>()
                    .NotNull();
                return (OctoVersionInfo)attribute!.TryGetValue(parameter, this)!;
            }

            return (OctoVersionInfo)(SValueCache[parameter] = SValueCache.GetValueOrDefault(parameter) ?? GetValue());
        }
    }
}