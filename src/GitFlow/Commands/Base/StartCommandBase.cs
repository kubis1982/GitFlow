using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;

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
