using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands.Base;

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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }
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
