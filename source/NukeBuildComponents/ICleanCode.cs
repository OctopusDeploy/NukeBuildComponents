using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.ReSharper;

namespace Octopus.NukeBuildComponents
{
    public interface ICleanCode : IOctopusNukeBuild
    {
        Target CleanCode => _ => _
            .OnlyWhenStatic(() => !IsLocalBuild)
            .TryTriggeredBy<IPackComponent>(x => x.Pack)
            .TryTriggeredBy<IPackExtension>(x => x.Pack)
            .Executes(() =>
            {
                ReSharperTasks.ReSharperCleanupCode(new ReSharperCleanupCodeSettings()
                    .SetTargetPath(Solution.Path));

                var currentBranch = GitRepository.FromLocalDirectory("./").Branch;
                if (currentBranch == null || currentBranch.StartsWith("prettybot/")) return;
                var prettyBotBranch = $"prettybot/{currentBranch}";

                if (prettyBotBranch is "main" or "master")
                {
                    Logger.Info("Doing anything automated to the default branch is not recommended. Exiting.");
                    return;
                }

                GitTasks.Git("config user.email \"bob@octopus.com\"");
                GitTasks.Git("config user.name \"prettybot[bot]\"");

                // Remove the local target branch, ignore if can't
                try
                {
                    GitTasks.Git($"show-ref --verify --quiet refs/heads/{prettyBotBranch}");
                    GitTasks.Git($"checkout -D {prettyBotBranch}");
                }
                catch
                {
                    // ignored
                }


                GitTasks.Git("status");
                var gitStatus = GitTasks.Git("status -s");
                if (gitStatus.Count == 0)
                {
                    var remote = GitTasks.Git($"ls-remote origin {prettyBotBranch}");
                    if (remote.Count == 0) GitTasks.Git($"push origin :{prettyBotBranch}");

                    return;
                }

                GitTasks.Git($"checkout -b {prettyBotBranch}");
                GitTasks.Git("add -A .");
                GitTasks.Git("commit -m \"Run ReSharper code cleanup\"");
                GitTasks.Git($"push -f --set-upstream origin {prettyBotBranch}");
            });
    }
}