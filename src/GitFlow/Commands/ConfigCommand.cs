using System.CommandLine;
using GitFlow.Services;
using GitFlow.Utilities;
using GitFlow.Models;

namespace GitFlow.Commands;

internal class ConfigCommand : Command
{
    public ConfigCommand() : base("config", "Manage GitFlow configuration")
    {
        Add(new InitCommand());
        Add(new TemplateCommand());
    }

    private static GitFlowConfig PromptForConfiguration(GitFlowConfig defaults, bool isGlobal = false)
    {
        Console.WriteLine();
        ConsoleHelper.PrintInfo(isGlobal ? "GitFlow Global Template Configuration" : "GitFlow Configuration");
        Console.WriteLine();

        // Production branch
        Console.Write($"Production branch [{defaults.ProductionBranch}]: ");
        var production = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(production))
            production = defaults.ProductionBranch;

        // Development branch
        Console.Write($"Development branch [{defaults.DevelopmentBranch}]: ");
        var development = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(development))
            development = defaults.DevelopmentBranch;

        string featurePrefix = defaults.FeaturePrefix;
        string releasePrefix = defaults.ReleasePrefix;
        string hotfixPrefix = defaults.HotfixPrefix;
        string bugfixPrefix = defaults.BugfixPrefix;

        Console.WriteLine();
        ConsoleHelper.PrintInfo("Branch Prefixes");
        Console.WriteLine();

        // Feature prefix
        Console.Write($"Feature branch prefix [{defaults.FeaturePrefix}]: ");
        var featurePrefixInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(featurePrefixInput))
            featurePrefix = featurePrefixInput;

        // Release prefix
        Console.Write($"Release branch prefix [{defaults.ReleasePrefix}]: ");
        var releasePrefixInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(releasePrefixInput))
            releasePrefix = releasePrefixInput;

        // Hotfix prefix
        Console.Write($"Hotfix branch prefix [{defaults.HotfixPrefix}]: ");
        var hotfixPrefixInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(hotfixPrefixInput))
            hotfixPrefix = hotfixPrefixInput;

        // Bugfix prefix
        Console.Write($"Bugfix branch prefix [{defaults.BugfixPrefix}]: ");
        var bugfixPrefixInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(bugfixPrefixInput))
            bugfixPrefix = bugfixPrefixInput;

        Console.WriteLine();
        ConsoleHelper.PrintInfo("Version Prefix");
        Console.WriteLine();

        // Version prefix
        var defaultVersionChoice = string.IsNullOrEmpty(defaults.VersionPrefix) ? "1" : "2";
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

        Console.WriteLine();
        ConsoleHelper.PrintInfo("Merge Strategy");
        Console.WriteLine();

        // Merge strategy
        var defaultMergeChoice = defaults.MergeStrategy switch
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

        return new GitFlowConfig
        {
            ProductionBranch = production,
            DevelopmentBranch = development,
            FeaturePrefix = featurePrefix,
            ReleasePrefix = releasePrefix,
            HotfixPrefix = hotfixPrefix,
            BugfixPrefix = bugfixPrefix,
            VersionPrefix = versionPrefix,
            MergeStrategy = mergeStrategy,
            IsGlobal = isGlobal
        };
    }

    private class InitCommand : Command
    {
        public InitCommand() : base("init", "Initialize GitFlow configuration in current repository")
        {
            var forceOption = new Option<bool>("-f", "--force")
            {
                Description = "Overwrite existing local configuration"
            };

            Add(forceOption);

            SetAction(n =>
            {
                try
                {
                    var force = n.GetValue(forceOption);

                    if (!GitRepositoryService.IsGitRepository())
                    {
                        ConsoleHelper.PrintError("Not a git repository. Initialize git first with 'git init'");
                        return;
                    }

                    var localConfig = ConfigurationService.ReadConfig(global: false);
                    var globalConfig = ConfigurationService.ReadConfig(global: true);

                    // Scenario 1: Local config exists - require --force
                    if (localConfig != null && !force)
                    {
                        ConsoleHelper.PrintError("GitFlow is already initialized in this repository. Use -f/--force to override.");
                        return;
                    }

                    GitFlowConfig config;

                    // Scenario 2: No local, but global template exists
                    if (localConfig == null && globalConfig != null)
                    {
                        Console.WriteLine();
                        ConsoleHelper.PrintInfo("GitFlow Configuration (from global template)");
                        Console.WriteLine();
                        Console.WriteLine($"  Production branch: {globalConfig.ProductionBranch}");
                        Console.WriteLine($"  Development branch: {globalConfig.DevelopmentBranch}");
                        Console.WriteLine($"  Feature prefix: {globalConfig.FeaturePrefix}");
                        Console.WriteLine($"  Release prefix: {globalConfig.ReleasePrefix}");
                        Console.WriteLine($"  Hotfix prefix: {globalConfig.HotfixPrefix}");
                        Console.WriteLine($"  Bugfix prefix: {globalConfig.BugfixPrefix}");
                        Console.WriteLine($"  Version prefix: {(string.IsNullOrEmpty(globalConfig.VersionPrefix) ? "<none>" : globalConfig.VersionPrefix)}");
                        Console.WriteLine($"  Merge strategy: {globalConfig.MergeStrategy}");
                        Console.WriteLine();
                        Console.Write("Accept this configuration? (Y/n): ");
                        var response = Console.ReadLine();
                        
                        if (string.IsNullOrWhiteSpace(response) || response.ToLower() == "y")
                        {
                            // Accept all - use global config as-is
                            config = new GitFlowConfig
                            {
                                ProductionBranch = globalConfig.ProductionBranch,
                                DevelopmentBranch = globalConfig.DevelopmentBranch,
                                FeaturePrefix = globalConfig.FeaturePrefix,
                                ReleasePrefix = globalConfig.ReleasePrefix,
                                HotfixPrefix = globalConfig.HotfixPrefix,
                                BugfixPrefix = globalConfig.BugfixPrefix,
                                VersionPrefix = globalConfig.VersionPrefix,
                                MergeStrategy = globalConfig.MergeStrategy,
                                IsGlobal = false
                            };
                        }
                        else
                        {
                            // Go through individual prompts with global template as defaults
                            config = PromptForConfiguration(globalConfig);
                        }
                    }
                    // Scenario 3: No local, no global - use system defaults
                    else if (localConfig == null)
                    {
                        var systemDefaults = new GitFlowConfig();
                        config = PromptForConfiguration(systemDefaults);
                    }
                    // Scenario 4: Force override existing local config
                    else
                    {
                        Console.WriteLine();
                        ConsoleHelper.PrintInfo("Reconfiguring GitFlow (current local configuration)");
                        Console.WriteLine();
                        config = PromptForConfiguration(localConfig);
                    }

                    ConfigurationService.WriteConfig(config, global: false);

                    Console.WriteLine();
                    ConsoleHelper.PrintSuccess("GitFlow initialized (local config)");
                    Console.WriteLine($"  Production branch: {config.ProductionBranch}");
                    Console.WriteLine($"  Development branch: {config.DevelopmentBranch}");
                    Console.WriteLine($"  Feature prefix: {config.FeaturePrefix}");
                    Console.WriteLine($"  Release prefix: {config.ReleasePrefix}");
                    Console.WriteLine($"  Hotfix prefix: {config.HotfixPrefix}");
                    Console.WriteLine($"  Bugfix prefix: {config.BugfixPrefix}");
                    Console.WriteLine($"  Version prefix: {(string.IsNullOrEmpty(config.VersionPrefix) ? "<none>" : config.VersionPrefix)}");
                    Console.WriteLine($"  Merge strategy: {config.MergeStrategy}");
                }
                catch (Exception ex)
                {
                    ConsoleHelper.PrintError($"Initialization failed: {ex.Message}");
                }
            });
        }
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

                    // Load existing config if available (for defaults), otherwise use system defaults
                    var existingConfig = ConfigurationService.ReadConfig(global: true) ?? new GitFlowConfig();

                    // Prompt with fixed prefixes (don't ask for them)
                    var config = PromptForConfiguration(existingConfig, isGlobal: true);

                    ConfigurationService.WriteConfig(config, global: true);

                    Console.WriteLine();
                    ConsoleHelper.PrintSuccess("Global GitFlow template configured successfully");
                    Console.WriteLine($"  Production branch: {config.ProductionBranch}");
                    Console.WriteLine($"  Development branch: {config.DevelopmentBranch}");
                    Console.WriteLine($"  Feature prefix: {config.FeaturePrefix}");
                    Console.WriteLine($"  Release prefix: {config.ReleasePrefix}");
                    Console.WriteLine($"  Hotfix prefix: {config.HotfixPrefix}");
                    Console.WriteLine($"  Bugfix prefix: {config.BugfixPrefix}");
                    Console.WriteLine($"  Version prefix: {(string.IsNullOrEmpty(config.VersionPrefix) ? "<none>" : config.VersionPrefix)}");
                    Console.WriteLine($"  Merge strategy: {config.MergeStrategy}");
                }
                catch (Exception ex)
                {
                    ConsoleHelper.PrintError($"Configuration failed: {ex.Message}");
                }
            });
        }
    }
}
