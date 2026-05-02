using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands.Base;

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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }
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
