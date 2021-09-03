using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.ValueInjection;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Octopus.NukeBuildComponents
{
    public interface ITest : IOctopusNukeBuild
    {
        [Parameter("Filter expression to run selective tests. Default is 'FullyQualifiedName!~Integration.Tests'")] 
        string TestFilter => ValueInjectionUtility.TryGetValue(() => TestFilter) ?? "FullyQualifiedName!~Integration.Tests";

        Target Test => _ => _
            .TryDependsOn<ICompile>(x => x.Compile)
            .Executes(() =>
            {
                DotNetTest(_ => _
                    .SetProjectFile(Solution)
                    .SetConfiguration(Config)
                    .SetNoBuild(true)
                    .EnableNoRestore()
                    .SetFilter(TestFilter));
            });
    }
}