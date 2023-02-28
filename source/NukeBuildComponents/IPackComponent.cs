using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Octopus.NukeBuildComponents
{
    public interface IPackComponent : IOctopusNukeBuild
    {
        Target Pack => _ => _
            .TryDependsOn<ITest>(x => x.Test)
            .Produces(ArtifactsDirectory / "*.nupkg")
            .Executes(() =>
            {
                Log.Information("Packing {1} v{0}", OctoVersionInfo.FullSemVer, TargetPackageDescription);

                // This is done to pass the data to github actions
                if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") is not null)
                {
                    var jobOutputFile = (AbsolutePath)Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
                    
                    File.AppendAllText(jobOutputFile, $"semver={OctoVersionInfo.FullSemVer}");
                    File.AppendAllText(jobOutputFile, $"prerelease_tag={OctoVersionInfo.PreReleaseTagWithDash}");
                }
                DotNetPack(_ => _
                    .SetProject(Solution)
                    .SetVersion(OctoVersionInfo.FullSemVer)
                    .SetConfiguration(Config)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .EnableNoBuild()
                    .DisableIncludeSymbols()
                    .SetVerbosity(DotNetVerbosity.Normal)
                    .SetProperty("NuspecProperties", $"Version={OctoVersionInfo.FullSemVer}"));
            });
    }
}