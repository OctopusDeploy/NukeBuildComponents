using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;

namespace Octopus.NukeBuildComponents
{
    public interface ICopyToLocalPackages : IOctopusNukeBuild
    {
        Target CopyToLocalPackages => _ => _
            .OnlyWhenStatic(() => IsLocalBuild)
            .TryTriggeredBy<IPackExtension>(x => x.Pack)
            .TryTriggeredBy<IPackComponent>(x => x.Pack)
            .Executes(() =>
            {
                EnsureExistingDirectory(LocalPackagesDir);
                ArtifactsDirectory.GlobFiles("*.nupkg")
                    .ForEach(package =>
                    {
                        CopyFileToDirectory(package, LocalPackagesDir, FileExistsPolicy.Overwrite);
                    });
            });
    }
}