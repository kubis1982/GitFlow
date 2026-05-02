using LibGit2Sharp;
using GitFlow.Models;

namespace GitFlow.Services;

public static class ConfigurationService
{
    public static GitFlowConfig? ReadConfig(bool global = false)
    {
        try
        {
            var repo = GitRepositoryService.GetRepository();
            
            if (global)
            {
                return ReadConfig(repo, ConfigurationLevel.Global);
            }

            return ReadConfig(repo, ConfigurationLevel.Local);
        }
        catch
        {
            return null;
        }
    }

    private static GitFlowConfig? ReadConfig(Repository repo, ConfigurationLevel configurationLevel)
    {
        try
        {
            var production = repo.Config.Get<string>("gitflow.production", configurationLevel)?.Value;
            var development = repo.Config.Get<string>("gitflow.development", configurationLevel)?.Value;

            if (production == null || development == null)
                return null;

            return new GitFlowConfig
            {
                ProductionBranch = production,
                DevelopmentBranch = development,
                FeaturePrefix = repo.Config.Get<string>("gitflow.prefix.feature", configurationLevel)?.Value ?? "feature/",
                ReleasePrefix = repo.Config.Get<string>("gitflow.prefix.release", configurationLevel)?.Value ?? "release/",
                HotfixPrefix = repo.Config.Get<string>("gitflow.prefix.hotfix", configurationLevel)?.Value ?? "hotfix/",
                BugfixPrefix = repo.Config.Get<string>("gitflow.prefix.bugfix", configurationLevel)?.Value ?? "bugfix/",
                VersionPrefix = repo.Config.Get<string>("gitflow.prefix.version", configurationLevel)?.Value ?? "",
                MergeStrategy = repo.Config.Get<string>("gitflow.merge.strategy", configurationLevel)?.Value ?? "--no-ff",
                IsGlobal = configurationLevel == ConfigurationLevel.Global
            };
        }
        catch
        {
            return null;
        }
    }

    public static bool ConfigExists(bool global = false)
    {
        return ReadConfig(global) != null;
    }

    public static void WriteConfig(GitFlowConfig config, bool global = false)
    {
        try
        {
            var repo = GitRepositoryService.GetRepository();
            
            if (global)
            {
                WriteConfig(repo, config, ConfigurationLevel.Global);
            }
            else
            {
                WriteConfig(repo, config, ConfigurationLevel.Local);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write GitFlow configuration: {ex.Message}", ex);
        }
    }

    private static void WriteConfig(Repository repo, GitFlowConfig config, ConfigurationLevel configurationLevel)
    {
        repo.Config.Set("gitflow.production", config.ProductionBranch, configurationLevel);
        repo.Config.Set("gitflow.development", config.DevelopmentBranch, configurationLevel);
        repo.Config.Set("gitflow.prefix.feature", config.FeaturePrefix, configurationLevel);
        repo.Config.Set("gitflow.prefix.release", config.ReleasePrefix, configurationLevel);
        repo.Config.Set("gitflow.prefix.hotfix", config.HotfixPrefix, configurationLevel);
        repo.Config.Set("gitflow.prefix.bugfix", config.BugfixPrefix, configurationLevel);
        
        // Handle version prefix - remove if empty, set if not
        if (string.IsNullOrEmpty(config.VersionPrefix))
            repo.Config.Unset("gitflow.prefix.version", configurationLevel);
        else
            repo.Config.Set("gitflow.prefix.version", config.VersionPrefix, configurationLevel);
            
        repo.Config.Set("gitflow.merge.strategy", config.MergeStrategy, configurationLevel);
    }
}
