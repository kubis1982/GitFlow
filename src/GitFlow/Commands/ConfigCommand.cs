using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;
using GitFlow.Models;

namespace GitFlow.Commands;

internal class ConfigCommand : Command
{
    public ConfigCommand() : base("config", "Manage GitFlow configuration")
    {
        Add(new TemplateCommand());
    }

    private class TemplateCommand : Command
    {
        public TemplateCommand() : base("template", "Set global GitFlow configuration template")
        {
            var forceOption = new Option<bool>("-f", "--force")
            {
                Description = "Overwrite existing global configuration"
            };

            Add(forceOption);

            SetAction(n =>
            {
                try
                {
                    var force = n.GetValue(forceOption);

                    if (ConfigurationService.ConfigExists(global: true) && !force)
                    {
                        ConsoleHelper.PrintError("Global GitFlow configuration already exists. Use -f/--force to override.");
                        return;
                    }

                    // Load existing config if available (for defaults)
                    var existingConfig = ConfigurationService.ReadConfig(global: true);

                    Console.WriteLine();
                    ConsoleHelper.PrintInfo("GitFlow Global Template Configuration");
                    Console.WriteLine();

                    // Production branch
                    var defaultProduction = existingConfig?.ProductionBranch ?? "main";
                    Console.Write($"Production branch [{defaultProduction}]: ");
                    var production = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(production))
                        production = defaultProduction;

                    // Development branch
                    var defaultDevelopment = existingConfig?.DevelopmentBranch ?? "develop";
                    Console.Write($"Development branch [{defaultDevelopment}]: ");
                    var development = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(development))
                        development = defaultDevelopment;

                    Console.WriteLine();
                    ConsoleHelper.PrintInfo("Merge Strategy");
                    Console.WriteLine();
                    
                    // Determine default merge strategy choice
                    var defaultMergeStrategy = existingConfig?.MergeStrategy ?? "--no-ff";
                    var defaultMergeChoice = defaultMergeStrategy switch
                    {
                        "--ff" => "2",
                        "--ff-only" => "3",
                        _ => "1"
                    };
                    
                    Console.WriteLine("Select merge strategy:");
                    Console.WriteLine("  1) --no-ff (recommended - always create merge commit)");
                    Console.WriteLine("  2) --ff (fast-forward if possible)");
                    Console.WriteLine("  3) --ff-only (only fast-forward, fail otherwise)");
                    Console.Write($"Choice [{defaultMergeChoice}]: ");
                    var mergeChoice = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(mergeChoice))
                        mergeChoice = defaultMergeChoice;
                    
                    string mergeStrategy = mergeChoice switch
                    {
                        "2" => "--ff",
                        "3" => "--ff-only",
                        _ => "--no-ff",
                    };
                    
                    Console.WriteLine();
                    ConsoleHelper.PrintInfo("Version Prefix");
                    Console.WriteLine();
                    
                    // Determine default version prefix choice
                    var defaultVersionPrefix = existingConfig?.VersionPrefix ?? "";
                    var defaultVersionChoice = string.IsNullOrEmpty(defaultVersionPrefix) ? "1" : "2";
                    
                    Console.WriteLine("Select version tag prefix:");
                    Console.WriteLine("  1) <none> (e.g., 1.0.0)");
                    Console.WriteLine("  2) v (e.g., v1.0.0)");                   
                    Console.Write($"Choice [{defaultVersionChoice}]: ");
                    var versionChoice = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(versionChoice))
                        versionChoice = defaultVersionChoice;
                    string versionPrefix = versionChoice switch
                    {
                        "2" => "v",
                        _ => "",
                    };
                    var config = new GitFlowConfig
                    {
                        ProductionBranch = production,
                        DevelopmentBranch = development,
                        FeaturePrefix = "feature/",
                        ReleasePrefix = "release/",
                        HotfixPrefix = "hotfix/",
                        BugfixPrefix = "bugfix/",
                        VersionPrefix = versionPrefix,
                        MergeStrategy = mergeStrategy,
                        IsGlobal = true
                    };

                    ConfigurationService.WriteConfig(config, global: true);

                    Console.WriteLine();
                    ConsoleHelper.PrintSuccess("Global GitFlow template configured successfully");
                    Console.WriteLine($"  Production branch: {production}");
                    Console.WriteLine($"  Development branch: {development}");
                    Console.WriteLine($"  Feature prefix: feature/");
                    Console.WriteLine($"  Release prefix: release/");
                    Console.WriteLine($"  Hotfix prefix: hotfix/");
                    Console.WriteLine($"  Bugfix prefix: bugfix/");
                    Console.WriteLine($"  Version prefix: {(string.IsNullOrEmpty(versionPrefix) ? "<none>" : versionPrefix)}");
                    Console.WriteLine($"  Merge strategy: {mergeStrategy}");
                }
                catch (Exception ex)
                {
                    ConsoleHelper.PrintError($"Configuration failed: {ex.Message}");
                }
            });
        }
    }
}
