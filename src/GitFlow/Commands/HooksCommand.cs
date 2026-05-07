using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands;

internal class HooksCommand : Command
{
    public HooksCommand() : base("hooks", "Manage GitFlow hooks")
    {
        Add(new HooksApplyCommand());
    }
}

internal class HooksApplyCommand : Command
{
    public HooksApplyCommand() : base("apply", "Apply hook templates in .git/hooks directory")
    {
        var templateArgument = new Argument<string>("template") 
        { 
            Description = "Template name: 'dotnet' or 'nodejs'" 
        };
        Add(templateArgument);

        var forceOption = new Option<bool>("-f", "--force")
        {
            Description = "Overwrite existing hooks"
        };
        Add(forceOption);

        SetAction(ctx =>
        {
            var template = ctx.GetValue(templateArgument)!;
            var force = ctx.GetValue(forceOption);
            
            try
            {
                // Validate repository
                var repo = GitRepositoryService.GetRepository();
                if (repo == null)
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                // Apply the template hooks
                var result = HookService.RegisterTemplate(repo, template, force);
                
                // Show summary
                Console.WriteLine();
                if (result.Copied > 0)
                {
                    ConsoleHelper.PrintSuccess($"✓ {result.Copied} hook(s) applied successfully");
                }
                
                if (result.Skipped > 0)
                {
                    ConsoleHelper.PrintInfo($"⊘ {result.Skipped} hook(s) skipped (already exist)");
                }
                
                if (result.Failed > 0)
                {
                    ConsoleHelper.PrintError($"✗ {result.Failed} hook(s) failed");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error applying hooks: {ex.Message}");
            }
        });
    }
}
