using System.CommandLine;
using GitFlow.Commands.Base;
using GitFlow.Models;
using GitFlow.Services;
using LibGit2Sharp;

namespace GitFlow.Commands;

internal class ReleaseCommand : Command
{
    public ReleaseCommand() : base("release", "Manage release branches")
    {
        Add(new ReleaseStartCommand());
        Add(new ReleasePublishCommand());
        Add(new ReleaseCheckoutCommand());
        Add(new ReleaseFinishCommand());
        Add(new ReleaseDeleteCommand());
        Add(new ReleaseUpdateCommand());
        Add(new ReleaseListCommand());
    }
}

internal class ReleaseStartCommand : StartCommandBase
{
    public ReleaseStartCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
    protected override string GetSourceBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class ReleasePublishCommand : PublishCommandBase
{
    public ReleasePublishCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
}

internal class ReleaseCheckoutCommand : CheckoutCommandBase
{
    public ReleaseCheckoutCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
}

internal class ReleaseFinishCommand : FinishCommandBase
{
    public ReleaseFinishCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
    
    protected override void PerformFinish(Repository repo, string branchName, GitFlowConfig config, string branchType)
    {
        MergeService.FinishRelease(repo, branchName, config);
    }
}

internal class ReleaseDeleteCommand : DeleteCommandBase
{
    public ReleaseDeleteCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
    protected override string GetTargetBranch(GitFlowConfig config) => config.ProductionBranch;
}

internal class ReleaseUpdateCommand : UpdateCommandBase
{
    public ReleaseUpdateCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
    protected override string GetParentBranch(GitFlowConfig config) => config.DevelopmentBranch;
}

internal class ReleaseListCommand : ListCommandBase
{
    public ReleaseListCommand() : base("release") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.ReleasePrefix;
}
