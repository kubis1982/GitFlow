using System.CommandLine;
using System.Diagnostics;
using GitFlow.Services;
using GitFlow.Utilities;
using GitFlow.Models;

namespace GitFlow.Commands;

internal class InitCommand : Command
{
    public InitCommand() : base("init", "Initialize GitFlow configuration")
    {
        var globalOption = new Option<bool>("-g", "--global")
        {
            Description = "Store configuration globally instead of locally"
        };

        var forceOption = new Option<bool>("-f", "--force")
        {
            Description = "Overwrite existing configuration"
        };

        var productionOption = new Option<string>("--production")
        {
            Description = "Production branch name",
            DefaultValueFactory = n => "main"
        };

        var developmentOption = new Option<string>("--development")
        {
            Description = "Development branch name",
            DefaultValueFactory = n => "develop"
        };

        var featurePrefixOption = new Option<string>("--feature-prefix")
        {
            Description = "Feature branch prefix",
            DefaultValueFactory = n => "feature/"
        };

        var releasePrefixOption = new Option<string>("--release-prefix")
        {
            Description = "Release branch prefix",
            DefaultValueFactory = n => "release/"
        };

        var hotfixPrefixOption = new Option<string>("--hotfix-prefix")
        {
            Description = "Hotfix branch prefix",
            DefaultValueFactory = n => "hotfix/"
        };

        var bugfixPrefixOption = new Option<string>("--bugfix-prefix")
        {
            Description = "Bugfix branch prefix",
            DefaultValueFactory = n => "bugfix/"
        };

        var versionPrefixOption = new Option<string>("--version-prefix")
        {
            Description = "Version tag prefix",
            DefaultValueFactory = n => "v"
        };

        var mergeStrategyOption = new Option<string>("--merge-strategy")
        {
            Description = "Merge strategy (--no-ff, squash, ff-only)",
            DefaultValueFactory = n => "--no-ff"
        };

        Options.Add(globalOption);
        Options.Add(forceOption);
        Options.Add(productionOption);
        Options.Add(developmentOption);
        Options.Add(featurePrefixOption);
        Options.Add(releasePrefixOption);
        Options.Add(hotfixPrefixOption);
        Options.Add(bugfixPrefixOption);
        Options.Add(versionPrefixOption);
        Options.Add(mergeStrategyOption);

        SetAction(n =>
        {
            try
            {
                var global = n.GetValue(globalOption);
                var force = n.GetValue(forceOption);
                var production = n.GetRequiredValue(productionOption);
                var development = n.GetRequiredValue(developmentOption);
                var featurePrefix = n.GetRequiredValue(featurePrefixOption);
                var releasePrefix = n.GetRequiredValue(releasePrefixOption);
                var hotfixPrefix = n.GetRequiredValue(hotfixPrefixOption);
                var bugfixPrefix = n.GetRequiredValue(bugfixPrefixOption);
                var versionPrefix = n.GetRequiredValue(versionPrefixOption);
                var mergeStrategy = n.GetRequiredValue(mergeStrategyOption);

                var gitService = new GitRepositoryService();
                var configService = new ConfigurationService(gitService);

                if (!gitService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository. Initialize git first with 'git init'");
                    return;
                }

                var repo = gitService.GetRepository();

                if (configService.ConfigExists(global) && !force)
                {
                    ConsoleHelper.PrintError("GitFlow is already initialized. Use -f/--force to override.");
                    return;
                }

                var config = new GitFlowConfig
                {
                    ProductionBranch = production,
                    DevelopmentBranch = development,
                    FeaturePrefix = featurePrefix,
                    ReleasePrefix = releasePrefix,
                    HotfixPrefix = hotfixPrefix,
                    BugfixPrefix = bugfixPrefix,
                    VersionPrefix = versionPrefix,
                    MergeStrategy = mergeStrategy,
                    IsGlobal = global
                };

                configService.WriteConfig(config, global);

                ConsoleHelper.PrintSuccess($"GitFlow initialized ({(global ? "global" : "local")} config)");
                Console.WriteLine($"  Production branch: {production}");
                Console.WriteLine($"  Development branch: {development}");
                Console.WriteLine($"  Feature prefix: {featurePrefix}");
                Console.WriteLine($"  Release prefix: {releasePrefix}");
                Console.WriteLine($"  Hotfix prefix: {hotfixPrefix}");
                Console.WriteLine($"  Bugfix prefix: {bugfixPrefix}");
                Console.WriteLine($"  Version prefix: {versionPrefix}");
                Console.WriteLine($"  Merge strategy: {mergeStrategy}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Initialization failed: {ex.Message}");
            }
        });
    }
}
