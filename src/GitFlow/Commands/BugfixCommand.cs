using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;
using GitFlow.Models;

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

internal class BugfixStartCommand : Command
{
    public BugfixStartCommand() : base("start", "Start a new bugfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                if (BranchService.BranchExists(repo, branchName))
                {
                    ConsoleHelper.PrintError($"Bugfix branch '{branchName}' already exists");
                    return;
                }

                BranchService.CreateBranch(repo, branchName, config.DevelopmentBranch);
                ConsoleHelper.PrintSuccess($"Bugfix branch '{branchName}' created");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixPublishCommand : Command
{
    public BugfixPublishCommand() : base("publish", "Publish a bugfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                BranchService.PublishBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Bugfix branch '{branchName}' published");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixCheckoutCommand : Command
{
    public BugfixCheckoutCommand() : base("checkout", "Checkout a bugfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                BranchService.CheckoutBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Checked out to '{branchName}'");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixFinishCommand : Command
{
    public BugfixFinishCommand() : base("finish", "Finish a bugfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                // Merge to development
                LibGit2Sharp.Commands.Checkout(repo, config.DevelopmentBranch);
                var branch = repo.Branches[branchName];
                var mergeOptions = new LibGit2Sharp.MergeOptions();
                var result = repo.Merge(branch, new LibGit2Sharp.Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), mergeOptions);

                if (result.Status == LibGit2Sharp.MergeStatus.Conflicts)
                {
                    ConsoleHelper.PrintError("Merge conflicts detected");
                    return;
                }

                BranchService.DeleteBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Bugfix branch '{branchName}' merged and deleted");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixDeleteCommand : Command
{
    public BugfixDeleteCommand() : base("delete", "Delete a bugfix branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                BranchService.DeleteBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Bugfix branch '{branchName}' deleted");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixUpdateCommand : Command
{
    public BugfixUpdateCommand() : base("update", "Update a bugfix branch with latest development changes")
    {
        var nameArgument = new Argument<string>("name") { Description = "Bugfix name" };
        Add(nameArgument);

        SetAction(n =>
        {
            try
            {
                var name = n.GetValue(nameArgument);
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branchName = config.BugfixPrefix + name;

                BranchService.UpdateBranch(repo, branchName, config.DevelopmentBranch);
                ConsoleHelper.PrintSuccess($"Bugfix branch '{branchName}' updated");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class BugfixListCommand : Command
{
    public BugfixListCommand() : base("list", "List all bugfix branches")
    {
        SetAction(n =>
        {
            try
            {
                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();
                var config = ConfigurationService.GetOrCreateConfig();
                var branches = BranchService.ListBranches(repo, config.BugfixPrefix);

                if (branches.Count == 0)
                {
                    Console.WriteLine("No bugfix branches found");
                    return;
                }

                Console.WriteLine("Bugfix branches:");
                foreach (var branch in branches)
                {
                    Console.WriteLine($"  {branch.FullName} ({branch.Tip})");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}
