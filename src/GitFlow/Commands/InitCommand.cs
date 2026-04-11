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

        Options.Add(globalOption);
        Options.Add(forceOption);

        SetAction(n =>
        {
            try
            {
                var global = n.GetValue(globalOption);
                var force = n.GetValue(forceOption);

                if (!GitRepositoryService.IsGitRepository())
                {
                    ConsoleHelper.PrintError("Not a git repository. Initialize git first with 'git init'");
                    return;
                }

                var repo = GitRepositoryService.GetRepository();

                if (ConfigurationService.ConfigExists(global) && !force)
                {
                    ConsoleHelper.PrintError("GitFlow is already initialized. Use -f/--force to override.");
                    return;
                }

                Console.WriteLine();
                ConsoleHelper.PrintInfo("GitFlow Configuration");
                Console.WriteLine();

                // Production branch
                Console.Write("Production branch [main]: ");
                var production = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(production))
                    production = "main";

                // Development branch
                Console.Write("Development branch [develop]: ");
                var development = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(development))
                    development = "develop";

                Console.WriteLine();
                ConsoleHelper.PrintInfo("Branch Prefixes");
                Console.WriteLine();

                // Feature prefix
                Console.Write("Feature branch prefix [feature/]: ");
                var featurePrefix = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(featurePrefix))
                    featurePrefix = "feature/";

                // Release prefix
                Console.Write("Release branch prefix [release/]: ");
                var releasePrefix = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(releasePrefix))
                    releasePrefix = "release/";

                // Hotfix prefix
                Console.Write("Hotfix branch prefix [hotfix/]: ");
                var hotfixPrefix = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(hotfixPrefix))
                    hotfixPrefix = "hotfix/";

                // Bugfix prefix
                Console.Write("Bugfix branch prefix [bugfix/]: ");
                var bugfixPrefix = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(bugfixPrefix))
                    bugfixPrefix = "bugfix/";

                // Version prefix
                Console.Write("Version tag prefix [v]: ");
                var versionPrefix = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(versionPrefix))
                    versionPrefix = "v";

                Console.WriteLine();
                ConsoleHelper.PrintInfo("Merge Strategy");
                Console.WriteLine();

                // Merge strategy
                Console.Write("Merge strategy [--no-ff]: ");
                var mergeStrategy = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(mergeStrategy))
                    mergeStrategy = "--no-ff";

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

                ConfigurationService.WriteConfig(config, global);

                Console.WriteLine();
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
