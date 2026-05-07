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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }

                var branchName = GetBranchPrefix(config) + name;
                var sourceBranch = GetSourceBranch(config);

                if (BranchService.BranchExists(repo, branchName))
                {
                    ConsoleHelper.PrintError($"{char.ToUpper(branchType[0]) + branchType.Substring(1)} branch '{branchName}' already exists");
                    return;
                }

                // Execute PRE hook (before branch creation)
                var preHookName = $"gitflow-{branchType}-start-pre.cs";
                if (HookService.HookExists(repo, preHookName))
                {
                    var preResult = HookService.ExecuteHook(repo, preHookName, branchName);
                    if (preResult is { Success: false })
                    {
                        ConsoleHelper.PrintError($"Pre-hook failed. Aborting {branchType} start.");
                        return;
                    }
                }

                BranchService.CreateBranch(repo, branchName, sourceBranch);

                // Execute POST hook (after branch creation)
                var postHookName = $"gitflow-{branchType}-start-post.cs";
                if (HookService.HookExists(repo, postHookName))
                {
                    var postResult = HookService.ExecuteHook(repo, postHookName, branchName);
                    if (postResult is { Success: false })
                    {
                        ConsoleHelper.PrintError($"Post-hook failed. Branch '{branchName}' was created but post-hook returned an error.");
                        return;
                    }
                }

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
