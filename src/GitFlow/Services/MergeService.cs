using LibGit2Sharp;
using GitFlow.Models;
using GitFlow.Utilities;

namespace GitFlow.Services;

public static class MergeService
{
    public static void FinishRelease(Repository repo, string branchName, GitFlowConfig config)
    {
        var releaseBranch = repo.Branches[branchName];
        if (releaseBranch == null)
            throw new InvalidOperationException($"Release branch '{branchName}' not found");

        ConsoleHelper.PrintInfo($"Starting finish process for release '{branchName}'...");

        // Extract version from branch name
        var version = branchName.Replace(config.ReleasePrefix, "");

        // Step 1: Verify working branch is up to date (blocking)
        BranchService.VerifyWorkingBranchIsUpToDate(repo, branchName);

        // Step 2: Ensure production and development branches are up to date
        BranchService.EnsureBranchIsUpToDate(repo, config.ProductionBranch);
        BranchService.EnsureBranchIsUpToDate(repo, config.DevelopmentBranch);

        // Step 3: Merge to production
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.ProductionBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.ProductionBranch);
        var signature = new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now);
        var prodResult = repo.Merge(releaseBranch, signature, new MergeOptions());
        
        if (prodResult.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException($"Merge conflicts detected with '{config.ProductionBranch}'. Please resolve manually.");
        }

        // Step 4: Create tag
        ConsoleHelper.PrintInfo($"Creating tag '{config.VersionPrefix}{version}'...");
        var tag = repo.Tags.Add($"{config.VersionPrefix}{version}", releaseBranch.Tip);
        
        // Step 5: Push production and tag to remote
        try
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                ConsoleHelper.PrintInfo($"Pushing '{config.ProductionBranch}' and tag to remote...");
                repo.Network.Push(remote, $"refs/heads/{config.ProductionBranch}");
                repo.Network.Push(remote, $"refs/tags/{config.VersionPrefix}{version}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintWarning($"Failed to push to remote: {ex.Message}");
        }

        // Step 6: Merge to development
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.DevelopmentBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        var devResult = repo.Merge(releaseBranch, signature, new MergeOptions());
        
        if (devResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintWarning($"Merge conflicts detected with '{config.DevelopmentBranch}'. Please resolve manually.");
            return;
        }

        // Step 7: Delete release branch (local and remote)
        ConsoleHelper.PrintInfo($"Deleting release branch '{branchName}'...");
        BranchService.DeleteBranch(repo, branchName, deleteRemote: true);

        // Step 8: Ensure we're on development branch
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);

        ConsoleHelper.PrintSuccess($"Release '{version}' finished successfully. Now on '{config.DevelopmentBranch}'.");
    }

    public static void FinishHotfix(Repository repo, string branchName, GitFlowConfig config)
    {
        var hotfixBranch = repo.Branches[branchName];
        if (hotfixBranch == null)
            throw new InvalidOperationException($"Hotfix branch '{branchName}' not found");

        ConsoleHelper.PrintInfo($"Starting finish process for hotfix '{branchName}'...");

        // Extract version from branch name
        var version = branchName.Replace(config.HotfixPrefix, "");

        // Step 1: Verify working branch is up to date (blocking)
        BranchService.VerifyWorkingBranchIsUpToDate(repo, branchName);

        // Step 2: Ensure production and development branches are up to date
        BranchService.EnsureBranchIsUpToDate(repo, config.ProductionBranch);
        BranchService.EnsureBranchIsUpToDate(repo, config.DevelopmentBranch);

        // Step 3: Merge to production
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.ProductionBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.ProductionBranch);
        var signature = new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now);
        var prodResult = repo.Merge(hotfixBranch, signature, new MergeOptions());
        
        if (prodResult.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException($"Merge conflicts detected with '{config.ProductionBranch}'. Please resolve manually.");
        }

        // Step 4: Create tag
        ConsoleHelper.PrintInfo($"Creating tag '{config.VersionPrefix}{version}'...");
        var tag = repo.Tags.Add($"{config.VersionPrefix}{version}", hotfixBranch.Tip);
        
        // Step 5: Push production and tag to remote
        try
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                ConsoleHelper.PrintInfo($"Pushing '{config.ProductionBranch}' and tag to remote...");
                repo.Network.Push(remote, $"refs/heads/{config.ProductionBranch}");
                repo.Network.Push(remote, $"refs/tags/{config.VersionPrefix}{version}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintWarning($"Failed to push to remote: {ex.Message}");
        }

        // Step 6: Merge to development
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.DevelopmentBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        var devResult = repo.Merge(hotfixBranch, signature, new MergeOptions());
        
        if (devResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintWarning($"Merge conflicts detected with '{config.DevelopmentBranch}'. Please resolve manually.");
            return;
        }

        // Step 7: Delete hotfix branch (local and remote)
        ConsoleHelper.PrintInfo($"Deleting hotfix branch '{branchName}'...");
        BranchService.DeleteBranch(repo, branchName, deleteRemote: true);

        // Step 8: Ensure we're on development branch
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);

        ConsoleHelper.PrintSuccess($"Hotfix '{version}' finished successfully. Now on '{config.DevelopmentBranch}'.");
    }
}
