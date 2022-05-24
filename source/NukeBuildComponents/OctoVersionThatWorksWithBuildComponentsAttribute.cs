#nullable disable
using System;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.OctoVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.ValueInjection;

namespace Octopus.NukeBuildComponents;

/// <summary>
///     Injects an instance of <see cref="OctoVersion" /> based on the local repository.
/// </summary>
/// <remarks>
///     This implementation is necessary, because over
///     <see
///         href="https://github.com/nuke-build/nuke/blob/develop/source/Nuke.Common/ValueInjection/ValueInjectionUtility.cs#L40">
///         here
///     </see>
///     , nuke passess null in
///     which goes :boom: over
///     <see
///         href="https://github.com/nuke-build/nuke/blob/develop/source/Nuke.Common/Tools/OctoVersion/OctoVersionAttribute.cs#L127">
///         here
///     </see>
///     , because we're not expecting it to be null
///     <see
///         href="https://github.com/nuke-build/nuke/blob/develop/source/Nuke.Common/ValueInjection/ValueInjectionAttributeBase.cs#L33">
///         here
///     </see>
/// </remarks>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
public class OctoVersionThatWorksWithBuildComponentsAttribute : ValueInjectionAttributeBase
{
    private bool? _autoDetectBranch;
    private int? _major;
    private int? _minor;
    private int? _patch;

    /// <summary>
    ///     Framework to use when selecting the OctoVersion library from the package.
    /// </summary>
    public string Framework { get; set; } = "net5.0";

    /// <summary>
    ///     Whether to update the build number and output OctoVersion results into the CI platform.
    ///     Supports AzurePipelines, TeamCity, AppVeyor, and GitHubActions.
    /// </summary>
    public bool UpdateBuildNumber { get; set; } = true;

    /// <summary>
    ///     Automatically detect the branch to explicitly pass to OctoVersion, based on the Git working copy.
    /// </summary>
    public bool AutoDetectBranch
    {
        get => throw new NotSupportedException();
        set => _autoDetectBranch = value;
    }

    /// <summary>
    ///     Name of the parameter specifying whether to auto detect the branch to explicitly pass to OctoVersion, based on the
    ///     Git working copy.
    ///     If this parameter is provided, it will override any value passed in the AutoDetectBranch property.
    /// </summary>
    [CanBeNull]
    public string AutoDetectBranchParameter { get; set; }

    /// <summary>
    ///     branch to pass to OctoVersion.
    /// </summary>
    [CanBeNull]
    public string Branch { get; set; }

    /// <summary>
    ///     Name of the parameter containing the branch to be passed to OctoVersion.
    ///     If this parameter is provided, it will override any value passed in the Branch property.
    /// </summary>
    [CanBeNull]
    public string BranchParameter { get; set; }

    /// <summary>
    ///     FullSemVer to pass to OctoVersion.
    /// </summary>
    [CanBeNull]
    public string FullSemVer { get; set; }

    /// <summary>
    ///     Name of the parameter containing the FullSemVer to be passed to OctoVersion.
    ///     If this parameter is provided, it will override any value passed in the FullSemVer property.
    /// </summary>
    [CanBeNull]
    public string FullSemVerParameter { get; set; }

    /// <summary>
    ///     Major version number to pass to OctoVersion..
    /// </summary>
    public int Major
    {
        get => throw new NotSupportedException();
        set => _major = value;
    }

    /// <summary>
    ///     Name of the parameter containing the Major version number to be passed to OctoVersion.
    ///     If this parameter is provided, it will override any value passed in the Major property.
    /// </summary>
    [CanBeNull]
    public string MajorParameter { get; set; }

    /// <summary>
    ///     Minor version number to pass to OctoVersion.
    /// </summary>
    public int Minor
    {
        get => throw new NotSupportedException();
        set => _minor = value;
    }

    /// <summary>
    ///     Name of the parameter containing the Minor version number to be passed to OctoVersion.
    ///     If this parameter is provided, it will override any value passed in the Minor property.
    /// </summary>
    [CanBeNull]
    public string MinorParameter { get; set; }

    /// <summary>
    ///     Patch version number to pass to OctoVersion.
    /// </summary>
    public int Patch
    {
        get => throw new NotSupportedException();
        set => _patch = value;
    }

    /// <summary>
    ///     Name of the parameter containing the Patch version number to be passed to OctoVersion.
    ///     If this parameter is provided, it will override any value passed in the Patch property
    /// </summary>
    [CanBeNull]
    public string PatchParameter { get; set; }

    public override object GetValue(MemberInfo member, object instance)
    {
        var autoDetectBranch = GetMemberValueOrNull<bool?>(AutoDetectBranchParameter, instance) ?? _autoDetectBranch;
        var branch = GetMemberValueOrNull<string>(BranchParameter, instance) ?? Branch;
        var fullSemVer = GetMemberValueOrNull<string>(FullSemVerParameter, instance) ?? FullSemVer;
        var majorVersion = GetMemberValueOrNull<int?>(MajorParameter, instance) ?? _major;
        var minorVersion = GetMemberValueOrNull<int?>(MinorParameter, instance) ?? _minor;
        var patchVersion = GetMemberValueOrNull<int?>(PatchParameter, instance) ?? _patch;

        Assert.False(autoDetectBranch.HasValue && autoDetectBranch.Value && !branch.IsNullOrEmpty(),
            $"Branch cannot be specified via {nameof(Branch)} or {nameof(BranchParameter)} properties when {nameof(AutoDetectBranch)} is enabled");
        Assert.True(autoDetectBranch.HasValue && autoDetectBranch.Value || !branch.IsNullOrEmpty(),
            $"Branch must either be provided via {nameof(Branch)} or {nameof(BranchParameter)} properties, or {nameof(AutoDetectBranch)} must be enabled");
        branch ??= GitRepository.FromLocalDirectory(NukeBuild.RootDirectory).Branch;

        var outputFile = NukeBuild.TemporaryDirectory / $"octoversion.{Guid.NewGuid()}.json";
        var version = OctoVersionTasks.OctoVersionGetVersion(_ => _
                .SetFramework(Framework)
                .SetOutputJsonFile(outputFile)
                .When(UpdateBuildNumber, _ => _
                    .EnableDetectEnvironment())
                .When(!UpdateBuildNumber, _ => _
                    .SetOutputFormats(OctoVersionOutputFormatter.Json))
                .SetCurrentBranch(branch)
                .SetFullSemVer(fullSemVer)
                .SetMajor(majorVersion)
                .SetMinor(minorVersion)
                .SetPatch(patchVersion))
            .Result;
        FileSystemTasks.DeleteFile(outputFile);

        if (UpdateBuildNumber)
        {
            AzurePipelines.Instance?.UpdateBuildNumber(version.FullSemVer);
            TeamCity.Instance?.SetBuildNumber(version.FullSemVer);
            AppVeyor.Instance?.UpdateBuildVersion(version.FullSemVer);
        }

        return version;
    }

    protected new T GetMemberValue<T>(string memberName, object instance)
    {
        //Note: this is the only real change we need here:
        var type = instance.GetType().GetInterface(nameof(IOctopusNukeBuild));
        var member = type!.GetMember(memberName, ReflectionUtility.All)
            .SingleOrDefaultOrError($"Found multiple members with the name '{memberName}' in '{type.Name}'")
            .NotNull($"No member '{memberName}' found in '{type.Name}'");
        Assert.True(typeof(T).IsAssignableFrom(member.GetMemberType()),
            $"Member '{type.Name}.{member.Name}' must be of type '{typeof(T).Name}'");
        return member.GetValue<T>(instance);
    }

    [CanBeNull]
    protected new T GetMemberValueOrNull<T>([CanBeNull] string memberName, object instance)
    {
        return memberName != null ? GetMemberValue<T>(memberName, instance) : default;
    }
}