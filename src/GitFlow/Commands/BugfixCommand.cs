using System.CommandLine;
using GitFlow.Commands.Base;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Commands;

internal class BugfixCommand : Command
{
    public BugfixCommand() : base("bugfix", "Manage bugfix branches")
    {
        Add(new BugfixStartCommand());
        Add(new BugfixPublishCommand());
        Add(new BugfixCheckoutCommand());
        Add(new BugfixFinishCommand());
        Add(new BugfixDeleteCommand());
        Add(new BugfixUpdateCommand());
        Add(new BugfixListCommand());
    }
}

internal class BugfixStartCommand : StartCommandBase
{
    public BugfixStartCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
    protected override string GetSourceBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class BugfixPublishCommand : PublishCommandBase
{
    public BugfixPublishCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
}

internal class BugfixCheckoutCommand : CheckoutCommandBase
{
    public BugfixCheckoutCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
}

internal class BugfixFinishCommand : FinishCommandBase
{
    public BugfixFinishCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
    
    protected override void PerformFinish(Repository repo, string branchName, GitFlowConfig config, string branchType)
    {
        ConsoleHelper.PrintInfo($"Starting finish process for bugfix '{branchName}'...");
        
        // Step 1: Verify working branch is up to date (blocking)
        BranchService.VerifyWorkingBranchIsUpToDate(repo, branchName);

        // Step 2: Ensure development branch is up to date
        BranchService.EnsureBranchIsUpToDate(repo, config.DevelopmentBranch);

        // Step 3: Merge to development
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.DevelopmentBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        
        var bugfixBranch = repo.Branches[branchName];
        var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
        
        var mergeOptions = new MergeOptions();
        if (config.MergeStrategy == "--no-ff")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.NoFastForward;
        else if (config.MergeStrategy == "squash")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.NoFastForward;
        else if (config.MergeStrategy == "ff-only")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.FastForwardOnly;

        var mergeResult = repo.Merge(bugfixBranch, signature, mergeOptions);

        if (mergeResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintError("Merge conflicts detected. Resolve conflicts and commit manually.");
            return;
        }

        // Step 4: Delete branch (local and remote)
        ConsoleHelper.PrintInfo($"Deleting bugfix branch '{branchName}'...");
        BranchService.DeleteBranch(repo, branchName, deleteRemote: true);

        // Step 5: Ensure we're on development branch
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);

        // Step 6: Push development branch to remote
        try
        {
            var remote = repo.Network.Remotes["origin"];
            if (remote != null)
            {
                ConsoleHelper.PrintInfo($"Pushing '{config.DevelopmentBranch}' to remote...");
                repo.Network.Push(remote, $"refs/heads/{config.DevelopmentBranch}");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintWarning($"Failed to push '{config.DevelopmentBranch}' to remote: {ex.Message}");
        }
        
        ConsoleHelper.PrintSuccess($"Bugfix '{branchName}' finished successfully and merged to '{config.DevelopmentBranch}'.");
    }
}

internal class BugfixDeleteCommand : DeleteCommandBase
{
    public BugfixDeleteCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
    protected override string GetTargetBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class BugfixUpdateCommand : UpdateCommandBase
{
    public BugfixUpdateCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
    protected override string GetParentBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class BugfixListCommand : ListCommandBase
{
    public BugfixListCommand() : base("bugfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.BugfixPrefix;
}
