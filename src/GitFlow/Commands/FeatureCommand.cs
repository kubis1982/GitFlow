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
        // Verify sync with remote
        if (!BranchService.LocalAndRemoteInSync(repo, branchName))
        {
            Console.Write("Local branch is not in sync with remote. Pull latest changes? (Y/n): ");
            var response = Console.ReadLine();
            if (response?.ToLower() != "n")
            {
                BranchService.CheckoutBranch(repo, branchName);
            }
        }

        // Merge to development
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

        // Delete branch
        BranchService.DeleteBranch(repo, branchName);
        ConsoleHelper.PrintSuccess($"Feature '{branchName}' finished and merged to {config.DevelopmentBranch}");
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
