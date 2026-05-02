using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Commands.Base;

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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }
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
