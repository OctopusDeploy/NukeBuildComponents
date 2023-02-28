using System;
using System.IO;
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

                // This is done to pass the data to github actions
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
                {
                    var jobOutputFile = (AbsolutePath)Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
                    File.AppendAllText(jobOutputFile, $"packages_to_push={string.Join(',', artifactPaths)}");
                }
                
                
            });
    }
}