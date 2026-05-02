using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands.Base;

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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }
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
