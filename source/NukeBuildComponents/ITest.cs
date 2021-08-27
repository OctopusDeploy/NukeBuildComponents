using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Octopus.NukeBuildComponents
{
    public interface ITest: IOctopusNukeBuild
    {
        Target Test => _ => _
            .TryDependsOn<ICompile>(x => x.Compile)
            .Executes(() =>
            {
                DotNetTest(_ => _
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetNoBuild(true)
                    .EnableNoRestore()
                    .SetFilter(@"FullyQualifiedName\!~Integration.Tests"));
            });
    }
}