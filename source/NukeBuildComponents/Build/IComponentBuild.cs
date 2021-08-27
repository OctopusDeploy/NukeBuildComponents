using Nuke.Common;

namespace Octopus.NukeBuildComponents
{
    public interface IComponentBuild :
        IRestore,
        IClean,
        ICompile,
        ICleanCode,
        ITest,
        IPackComponent,
        IOutputPackagesToPush,
        ICopyToLocalPackages
    {
        Target Default => _ => _
            .TryDependsOn<IOutputPackagesToPush>(x => x.OutputPackagesToPush);
    }
}