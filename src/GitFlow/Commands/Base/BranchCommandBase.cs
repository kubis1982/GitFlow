using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Commands.Base;

internal abstract class StartCommandBase : Command
{
    protected StartCommandBase(string branchType) : base("start", $"Start a new {branchType} branch")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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

                var branchName = GetBranchPrefix(config) + name;
                var sourceBranch = GetSourceBranch(config);

                if (BranchService.BranchExists(repo, branchName))
                {
                    ConsoleHelper.PrintError($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' already exists");
                    return;
                }

                BranchService.CreateBranch(repo, branchName, sourceBranch);
                ConsoleHelper.PrintSuccess($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' created");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
    protected abstract string GetSourceBranch(GitFlowConfig config);
}

internal abstract class PublishCommandBase : Command
{
    protected PublishCommandBase(string branchType) : base("publish", $"Publish a {branchType} branch")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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
                var branchName = GetBranchPrefix(config) + name;

                BranchService.PublishBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' published");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
}

internal abstract class CheckoutCommandBase : Command
{
    protected CheckoutCommandBase(string branchType) : base("checkout", $"Checkout a {branchType} branch")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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
                var branchName = GetBranchPrefix(config) + name;

                BranchService.CheckoutBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"Checked out {branchType} branch '{branchName}'");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
}

internal abstract class FinishCommandBase : Command
{
    protected FinishCommandBase(string branchType) : base("finish", $"Finish a {branchType} branch")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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
                var branchName = GetBranchPrefix(config) + name;

                PerformFinish(repo, branchName, config, branchType);
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
    protected abstract void PerformFinish(Repository repo, string branchName, GitFlowConfig config, string branchType);
}

internal abstract class DeleteCommandBase : Command
{
    protected DeleteCommandBase(string branchType) : base("delete", $"Delete a {branchType} branch")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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
                var branchName = GetBranchPrefix(config) + name;

                var targetBranch = GetTargetBranch(config);
                if (BranchService.BranchHasUnmergedCommits(repo, branchName, targetBranch))
                {
                    Console.Write($"Branch '{branchName}' has unmerged commits. Delete anyway? (y/N): ");
                    var response = Console.ReadLine();
                    if (response?.ToLower() != "y")
                    {
                        ConsoleHelper.PrintWarning("Delete cancelled");
                        return;
                    }
                }

                BranchService.DeleteBranch(repo, branchName);
                ConsoleHelper.PrintSuccess($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' deleted");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
    protected abstract string GetTargetBranch(GitFlowConfig config);
}

internal abstract class UpdateCommandBase : Command
{
    protected UpdateCommandBase(string branchType) : base("update", $"Update a {branchType} branch with latest changes")
    {
        var nameArgument = new Argument<string>("name") { Description = $"{branchType} name" };
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
                var branchName = GetBranchPrefix(config) + name;
                var parentBranch = GetParentBranch(config);

                BranchService.UpdateBranch(repo, branchName, parentBranch);
                ConsoleHelper.PrintSuccess($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' updated with latest {parentBranch}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
    protected abstract string GetParentBranch(GitFlowConfig config);
}

internal abstract class ListCommandBase : Command
{
    protected ListCommandBase(string branchType) : base("list", $"List all {branchType} branches")
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
                var prefix = GetBranchPrefix(config);

                var branches = BranchService.ListBranches(repo, prefix);

                if (branches.Count == 0)
                {
                    ConsoleHelper.PrintWarning($"No {branchType} branches found");
                    return;
                }

                ConsoleHelper.PrintInfo($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branches:");
                foreach (var branch in branches)
                {
                    var marker = branch.IsCurrentBranch ? "* " : "  ";
                    var location = "";
                    
                    if (branch.IsLocal && branch.IsRemote)
                    {
                        if (branch.IsTracking)
                        {
                            if (branch.AheadBy > 0 && branch.BehindBy > 0)
                                location = $" [↕ {branch.AheadBy}↑ {branch.BehindBy}↓]";
                            else if (branch.AheadBy > 0)
                                location = $" [↑ {branch.AheadBy}]";
                            else if (branch.BehindBy > 0)
                                location = $" [↓ {branch.BehindBy}]";
                            else
                                location = " [↕]";
                        }
                        else
                        {
                            location = " [local+remote]";
                        }
                    }
                    else if (branch.IsRemote && !branch.IsLocal)
                    {
                        location = " [remote-only]";
                    }
                    
                    Console.WriteLine($"{marker}{branch.Name}{location}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        });
    }

    protected abstract string GetBranchPrefix(GitFlowConfig config);
}
