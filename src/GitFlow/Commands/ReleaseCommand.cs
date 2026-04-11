using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;

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

internal class ReleaseStartCommand : Command
{
    public ReleaseStartCommand() : base("start", "Start a new release branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            BranchService.CreateBranch(repo, branchName, config.DevelopmentBranch);
            ConsoleHelper.PrintSuccess($"Release branch '{branchName}' created");
        });
    }
}

internal class ReleasePublishCommand : Command
{
    public ReleasePublishCommand() : base("publish", "Publish a release branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            BranchService.PublishBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Release branch '{branchName}' published");
        });
    }
}

internal class ReleaseCheckoutCommand : Command
{
    public ReleaseCheckoutCommand() : base("checkout", "Checkout a release branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            BranchService.CheckoutBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Checked out to '{branchName}'");
        });
    }
}

internal class ReleaseFinishCommand : Command
{
    public ReleaseFinishCommand() : base("finish", "Finish a release branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            MergeService.FinishRelease(repo, branchName, config);
        });
    }
}

internal class ReleaseDeleteCommand : Command
{
    public ReleaseDeleteCommand() : base("delete", "Delete a release branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            BranchService.DeleteBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Release branch '{branchName}' deleted");
        });
    }
}

internal class ReleaseUpdateCommand : Command
{
    public ReleaseUpdateCommand() : base("update", "Update a release branch with latest development changes")
    {
        var nameArgument = new Argument<string>("name") { Description = "Release version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branchName = config.ReleasePrefix + name;

            BranchService.UpdateBranch(repo, branchName, config.DevelopmentBranch);
            ConsoleHelper.PrintSuccess($"Release branch '{branchName}' updated");
        });
    }
}

internal class ReleaseListCommand : Command
{
    public ReleaseListCommand() : base("list", "List all release branches")
    {
        SetAction(n =>
        {
            var repo = GitRepositoryService.GetRepository();
            var config = ConfigurationService.GetOrCreateConfig();
            var branches = BranchService.ListBranches(repo, config.ReleasePrefix);

            if (branches.Count == 0)
            {
                Console.WriteLine("No release branches found");
                return;
            }

            Console.WriteLine("Release branches:");
            foreach (var branch in branches)
            {
                Console.WriteLine($"  {branch.FullName} ({branch.Tip})");
            }
        });
    }
}
