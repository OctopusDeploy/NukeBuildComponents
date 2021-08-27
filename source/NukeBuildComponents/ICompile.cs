using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


namespace Octopus.NukeBuildComponents
{
    public interface ICompile : IOctopusNukeBuild
    {
        Target Compile => _ => _
            .TryDependsOn<IClean>(x => x.Clean)
            .TryDependsOn<IRestore>(x => x.Restore)
            .Executes(() =>
            {
                Logger.Info("Building AzureDevOps issue tracker v{0}", OctoVersionInfo.FullSemVer);

                DotNetBuild(_ => _
                    .SetProjectFile(Solution)
                    .SetConfiguration(Config)
                    .SetVersion(OctoVersionInfo.FullSemVer)
                    .EnableNoRestore());
                
            });
    }
}