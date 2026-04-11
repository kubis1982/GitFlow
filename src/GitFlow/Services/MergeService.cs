using LibGit2Sharp;
using GitFlow.Models;
using GitFlow.Utilities;

namespace GitFlow.Services;

public class MergeService(GitRepositoryService gitService, BranchService branchService)
{
    public void FinishRelease(Repository repo, string branchName, GitFlowConfig config)
    {
        var releaseBranch = repo.Branches[branchName];
        if (releaseBranch == null)
            throw new InvalidOperationException($"Release branch '{branchName}' not found");

        // Extract version from branch name
        var version = branchName.Replace(config.ReleasePrefix, "");

        // 1. Merge to production
        ConsoleHelper.PrintInfo("Merging to production branch...");
        LibGit2Sharp.Commands.Checkout(repo, config.ProductionBranch);
        var prodResult = repo.Merge(releaseBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), new MergeOptions());
        
        if (prodResult.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException("Merge conflicts with production branch");
        }

        // 2. Create tag
        ConsoleHelper.PrintInfo($"Creating tag {config.VersionPrefix}{version}...");
        var tag = repo.Tags.Add($"{config.VersionPrefix}{version}", releaseBranch.Tip);
        
        // 3. Push production and tag
        try
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                repo.Network.Push(remote, $"refs/heads/{config.ProductionBranch}");
                repo.Network.Push(remote, $"refs/tags/{config.VersionPrefix}{version}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintWarning($"Failed to push to remote: {ex.Message}");
        }

        // 4. Merge to development
        ConsoleHelper.PrintInfo("Merging to development branch...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        var devResult = repo.Merge(releaseBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), new MergeOptions());
        
        if (devResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintWarning("Merge conflicts with development branch - resolve manually");
        }

        // 5. Delete release branch
        ConsoleHelper.PrintInfo("Deleting release branch...");
        branchService.DeleteBranch(repo, branchName);

        ConsoleHelper.PrintSuccess($"Release '{version}' finished successfully");
    }

    public void FinishHotfix(Repository repo, string branchName, GitFlowConfig config)
    {
        var hotfixBranch = repo.Branches[branchName];
        if (hotfixBranch == null)
            throw new InvalidOperationException($"Hotfix branch '{branchName}' not found");

        // Extract version from branch name
        var version = branchName.Replace(config.HotfixPrefix, "");

        // 1. Merge to production
        ConsoleHelper.PrintInfo("Merging to production branch...");
        LibGit2Sharp.Commands.Checkout(repo, config.ProductionBranch);
        var prodResult = repo.Merge(hotfixBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), new MergeOptions());
        
        if (prodResult.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException("Merge conflicts with production branch");
        }

        // 2. Create tag
        ConsoleHelper.PrintInfo($"Creating tag {config.VersionPrefix}{version}...");
        var tag = repo.Tags.Add($"{config.VersionPrefix}{version}", hotfixBranch.Tip);
        
        // 3. Push production and tag
        try
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                repo.Network.Push(remote, $"refs/heads/{config.ProductionBranch}");
                repo.Network.Push(remote, $"refs/tags/{config.VersionPrefix}{version}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintWarning($"Failed to push to remote: {ex.Message}");
        }

        // 4. Merge to development
        ConsoleHelper.PrintInfo("Merging to development branch...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        var devResult = repo.Merge(hotfixBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), new MergeOptions());
        
        if (devResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintWarning("Merge conflicts with development branch - resolve manually");
        }

        // 5. Delete hotfix branch
        ConsoleHelper.PrintInfo("Deleting hotfix branch...");
        branchService.DeleteBranch(repo, branchName);

        ConsoleHelper.PrintSuccess($"Hotfix '{version}' finished successfully");
    }
}
