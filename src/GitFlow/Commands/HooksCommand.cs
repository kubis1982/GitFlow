using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;

namespace GitFlow.Commands;

internal class HooksCommand : Command
{
    public HooksCommand() : base("hooks", "Manage GitFlow hooks")
    {
        Add(new HooksRegisterCommand());
    }
}

internal class HooksRegisterCommand : Command
{
    public HooksRegisterCommand() : base("register", "Register hooks from templates")
    {
        var templateArgument = new Argument<string>("template") 
        { 
            Description = "Template name: 'dotnet' or 'nodejs'" 
        };
        Add(templateArgument);

        SetAction(ctx =>
        {
            var template = ctx.GetValue(templateArgument)!;
            
            try
            {
                // Validate repository
                var repo = GitRepositoryService.GetRepository();
                if (repo == null)
                {
                    ConsoleHelper.PrintError("Not a git repository");
                    return;
                }

                // Register the template hooks
                var result = HookService.RegisterTemplate(repo, template);
                
                // Show summary
                Console.WriteLine();
                if (result.Copied > 0)
                {
                    ConsoleHelper.PrintSuccess($"✓ {result.Copied} hook(s) registered successfully");
                }
                
                if (result.Skipped > 0)
                {
                    ConsoleHelper.PrintInfo($"⊘ {result.Skipped} hook(s) skipped (already exist)");
                }
                
                if (result.Failed > 0)
                {
                    ConsoleHelper.PrintError($"✗ {result.Failed} hook(s) failed validation");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error registering hooks: {ex.Message}");
            }
        });
    }
}
