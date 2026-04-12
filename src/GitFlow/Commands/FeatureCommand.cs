using System.CommandLine;
using GitFlow.Commands.Base;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Commands;

internal class FeatureCommand : Command
{
    public FeatureCommand() : base("feature", "Manage feature branches")
    {
        Add(new FeatureStartCommand());
        Add(new FeaturePublishCommand());
        Add(new FeatureCheckoutCommand());
        Add(new FeatureFinishCommand());
        Add(new FeatureDeleteCommand());
        Add(new FeatureUpdateCommand());
        Add(new FeatureListCommand());
    }
}

internal class FeatureStartCommand : StartCommandBase
{
    public FeatureStartCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
    protected override string GetSourceBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class FeaturePublishCommand : PublishCommandBase
{
    public FeaturePublishCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
}

internal class FeatureCheckoutCommand : CheckoutCommandBase
{
    public FeatureCheckoutCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
}

internal class FeatureFinishCommand : FinishCommandBase
{
    public FeatureFinishCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
    
    protected override void PerformFinish(Repository repo, string branchName, GitFlowConfig config, string branchType)
    {
        ConsoleHelper.PrintInfo($"Starting finish process for feature '{branchName}'...");
        
        // Step 1: Verify working branch is up to date (blocking)
        BranchService.VerifyWorkingBranchIsUpToDate(repo, branchName);

        // Step 2: Ensure development branch is up to date
        BranchService.EnsureBranchIsUpToDate(repo, config.DevelopmentBranch);

        // Step 3: Merge to development
        ConsoleHelper.PrintInfo($"Merging '{branchName}' to '{config.DevelopmentBranch}'...");
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        
        var featureBranch = repo.Branches[branchName];
        var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
        
        var mergeOptions = new MergeOptions();
        if (config.MergeStrategy == "--no-ff")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.NoFastForward;
        else if (config.MergeStrategy == "squash")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.NoFastForward;
        else if (config.MergeStrategy == "ff-only")
            mergeOptions.FastForwardStrategy = FastForwardStrategy.FastForwardOnly;

        var mergeResult = repo.Merge(featureBranch, signature, mergeOptions);

        if (mergeResult.Status == MergeStatus.Conflicts)
        {
            ConsoleHelper.PrintError("Merge conflicts detected. Resolve conflicts and commit manually.");
            return;
        }

        // Step 4: Delete branch (local and remote)
        ConsoleHelper.PrintInfo($"Deleting feature branch '{branchName}'...");
        BranchService.DeleteBranch(repo, branchName, deleteRemote: true);

        // Step 5: Ensure we're on development branch
        LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
        
        ConsoleHelper.PrintSuccess($"Feature '{branchName}' finished successfully and merged to '{config.DevelopmentBranch}'.");
    }
}

internal class FeatureDeleteCommand : DeleteCommandBase
{
    public FeatureDeleteCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
    protected override string GetTargetBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class FeatureUpdateCommand : UpdateCommandBase
{
    public FeatureUpdateCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
    protected override string GetParentBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class FeatureListCommand : ListCommandBase
{
    public FeatureListCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
}
