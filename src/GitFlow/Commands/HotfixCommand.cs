using System.CommandLine;
using GitFlow.Commands.Base;
using GitFlow.Models;
using GitFlow.Services;
using LibGit2Sharp;

namespace GitFlow.Commands;

internal class HotfixCommand : Command
{
    public HotfixCommand() : base("hotfix", "Manage hotfix branches")
    {
        Add(new HotfixStartCommand());
        Add(new HotfixPublishCommand());
        Add(new HotfixCheckoutCommand());
        Add(new HotfixFinishCommand());
        Add(new HotfixDeleteCommand());
        Add(new HotfixUpdateCommand());
        Add(new HotfixListCommand());
    }
}

internal class HotfixStartCommand : StartCommandBase
{
    public HotfixStartCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
    protected override string GetSourceBranch(GitFlowConfig config) => config.ProductionBranch;
}

internal class HotfixPublishCommand : PublishCommandBase
{
    public HotfixPublishCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
}

internal class HotfixCheckoutCommand : CheckoutCommandBase
{
    public HotfixCheckoutCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
}

internal class HotfixFinishCommand : FinishCommandBase
{
    public HotfixFinishCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
    
    protected override void PerformFinish(Repository repo, string branchName, GitFlowConfig config, string branchType)
    {
        MergeService.FinishHotfix(repo, branchName, config);
    }
}

internal class HotfixDeleteCommand : DeleteCommandBase
{
    public HotfixDeleteCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
    protected override string GetTargetBranch(GitFlowConfig config) => config.ProductionBranch;
}

internal class HotfixUpdateCommand : UpdateCommandBase
{
    public HotfixUpdateCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
    protected override string GetParentBranch(GitFlowConfig config) => config.ProductionBranch;
}

internal class HotfixListCommand : ListCommandBase
{
    public HotfixListCommand() : base("hotfix") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.HotfixPrefix;
}
