using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Octopus.NukeBuildComponents
{
    public interface IRestore : IOctopusNukeBuild
    {
        Target Restore => _ => _
            .TryDependsOn<IClean>(x => x.Clean)
            .Executes(() =>
            {
                DotNetRestore(_ => _
                    .SetProjectFile(Solution));
            });
    }
}