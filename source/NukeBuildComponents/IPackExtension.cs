using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Octopus.NukeBuildComponents
{
    public interface IPackExtension : IOctopusNukeBuild
    {
        public string NuspecFilePath { get; }

        Target Pack => _ => _
            .TryDependsOn<ITest>(x => x.Test)
            .Produces(ArtifactsDirectory / "*.nupkg")
            .Executes(() =>
            {
                Logger.Info("Packing AzureDevOps issue tracker v{0}", OctoVersionInfo.FullSemVer);

                // This is done to pass the data to github actions
                Console.Out.WriteLine($"::set-output name=semver::{OctoVersionInfo.FullSemVer}");
                Console.Out.WriteLine($"::set-output name=prerelease_tag::{OctoVersionInfo.PreReleaseTagWithDash}");

                DotNetPack(_ => _
                    .SetProject(Solution)
                    .SetVersion(OctoVersionInfo.FullSemVer)
                    .SetConfiguration(Config)
                    .SetOutputDirectory(ArtifactsDirectory)
                    .EnableNoBuild()
                    .DisableIncludeSymbols()
                    .SetVerbosity(DotNetVerbosity.Normal)
                    .SetProperty("NuspecFile", NuspecFilePath)
                    .SetProperty("NuspecProperties", $"Version={OctoVersionInfo.FullSemVer}"));

                DotNetPack(_ => _
                    .SetProject(RootDirectory / "source/Client/Client.csproj")
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