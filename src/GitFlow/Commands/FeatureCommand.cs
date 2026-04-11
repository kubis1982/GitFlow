using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;
using GitFlow.Models;

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

internal class FeatureStartCommand : Command
{
    public FeatureStartCommand() : base("start", "Start a new feature branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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

                var branchName = config.FeaturePrefix + name;

                if (BranchService.BranchExists(repo, branchName))
                {
                    ConsoleHelper.PrintError($"Feature branch '{branchName}' already exists");
                    return;
                }

                BranchService.CreateBranch(repo, branchName, config.DevelopmentBranch);
                ConsoleHelper.PrintSuccess($"Feature branch '{branchName}' created");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class FeaturePublishCommand : Command
{
    public FeaturePublishCommand() : base("publish", "Publish a feature branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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
                var branchName = config.FeaturePrefix + name;

                BranchService.PublishBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Feature branch '{branchName}' published");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class FeatureCheckoutCommand : Command
{
    public FeatureCheckoutCommand() : base("checkout", "Checkout a feature branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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
                var branchName = config.FeaturePrefix + name;

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

internal class FeatureFinishCommand : Command
{
    public FeatureFinishCommand() : base("finish", "Finish a feature branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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
                var branchName = config.FeaturePrefix + name;

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

                // Delete branch
                BranchService.DeleteBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Feature branch '{branchName}' merged and deleted");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class FeatureDeleteCommand : Command
{
    public FeatureDeleteCommand() : base("delete", "Delete a feature branch")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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
                var branchName = config.FeaturePrefix + name;

                BranchService.DeleteBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Feature branch '{branchName}' deleted");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class FeatureUpdateCommand : Command
{
    public FeatureUpdateCommand() : base("update", "Update a feature branch with latest development changes")
    {
        var nameArgument = new Argument<string>("name") { Description = "Feature name" };
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
                var branchName = config.FeaturePrefix + name;

                BranchService.UpdateBranch(repo, branchName, config.DevelopmentBranch);
                ConsoleHelper.PrintSuccess($"Feature branch '{branchName}' updated");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }
}

internal class FeatureListCommand : Command
{
    public FeatureListCommand() : base("list", "List all feature branches")
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
                var branches = BranchService.ListBranches(repo, config.FeaturePrefix);

                if (branches.Count == 0)
                {
                    Console.WriteLine("No feature branches found");
                    return;
                }

                Console.WriteLine("Feature branches:");
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
