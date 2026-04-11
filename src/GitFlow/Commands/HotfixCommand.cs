using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;

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

internal class HotfixStartCommand : Command
{
    public HotfixStartCommand() : base("start", "Start a new hotfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            BranchService.CreateBranch(repo, branchName, config.ProductionBranch);
            ConsoleHelper.PrintSuccess($"Hotfix branch '{branchName}' created from production");
        });
    }
}

internal class HotfixPublishCommand : Command
{
    public HotfixPublishCommand() : base("publish", "Publish a hotfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            BranchService.PublishBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Hotfix branch '{branchName}' published");
        });
    }
}

internal class HotfixCheckoutCommand : Command
{
    public HotfixCheckoutCommand() : base("checkout", "Checkout a hotfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            BranchService.CheckoutBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Checked out to '{branchName}'");
        });
    }
}

internal class HotfixFinishCommand : Command
{
    public HotfixFinishCommand() : base("finish", "Finish a hotfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            MergeService.FinishHotfix(repo, branchName, config);
        });
    }
}

internal class HotfixDeleteCommand : Command
{
    public HotfixDeleteCommand() : base("delete", "Delete a hotfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            BranchService.DeleteBranch(repo, branchName);
            ConsoleHelper.PrintSuccess($"Hotfix branch '{branchName}' deleted");
        });
    }
}

internal class HotfixUpdateCommand : Command
{
    public HotfixUpdateCommand() : base("update", "Update a hotfix branch with latest production changes")
    {
        var nameArgument = new Argument<string>("name") { Description = "Hotfix version" };
        Add(nameArgument);

        SetAction(n =>
        {
            var name = n.GetValue(nameArgument);
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branchName = config.HotfixPrefix + name;

            BranchService.UpdateBranch(repo, branchName, config.ProductionBranch);
            ConsoleHelper.PrintSuccess($"Hotfix branch '{branchName}' updated");
        });
    }
}

internal class HotfixListCommand : Command
{
    public HotfixListCommand() : base("list", "List all hotfix branches")
    {
        SetAction(n =>
        {
            var configService = new ConfigurationService();

            var repo = GitRepositoryService.GetRepository();
            var config = configService.GetOrCreateConfig();
            var branches = BranchService.ListBranches(repo, config.HotfixPrefix);

            if (branches.Count == 0)
            {
                Console.WriteLine("No hotfix branches found");
                return;
            }

            Console.WriteLine("Hotfix branches:");
            foreach (var branch in branches)
            {
                Console.WriteLine($"  {branch.FullName} ({branch.Tip})");
            }
        });
    }
}
