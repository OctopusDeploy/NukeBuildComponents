using Nuke.Common;

namespace Octopus.NukeBuildComponents
{
    public interface IExtensionBuild :
        IRestore,
        IClean,
        ICompile,
        ICleanCode,
        ITest,
        IPackExtension,
        IOutputPackagesToPush,
        ICopyToLocalPackages
    {
        Target Default => _ => _
            .TryDependsOn<IOutputPackagesToPush>(x => x.OutputPackagesToPush);
    }
}