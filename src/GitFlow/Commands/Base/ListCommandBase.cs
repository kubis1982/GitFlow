using System.CommandLine;
using GitFlow.Models;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands.Base;

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
                var config = ConfigurationService.ReadConfig(false);
                if (config == null)
                {
                    ConsoleHelper.PrintError("GitFlow is not initialized in this repository. Run 'gitflow config init' first.");
                    return;
                }
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
