using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;

namespace Octopus.NukeBuildComponents
{
    public interface IOutputPackagesToPush : IOctopusNukeBuild
    {
        Target OutputPackagesToPush => _ => _
            .TryDependsOn<IPackExtension>(x => x.Pack)
            .TryDependsOn<IPackComponent>(x => x.Pack)
            .Executes(() =>
            {
                var artifactPaths = ArtifactsDirectory.GlobFiles("*.nupkg")
                    .Where(p => p is not null)
                    .Select(p => p.ToString());

                Console.WriteLine($"::set-output name=packages_to_push::{string.Join(',', artifactPaths)}");
            });
    }
}