using LibGit2Sharp;
using GitFlow.Models;
using System.Diagnostics;

namespace GitFlow.Services;

public class ConfigurationService
{
    public GitFlowConfig? ReadConfig(bool global = false)
    {
        try
        {
            if (global)
            {
                return ReadGlobalConfig();
            }

            var repo = GitRepositoryService.GetRepository();
            return ReadLocalConfig(repo);
        }
        catch
        {
            return null;
        }
    }

    public GitFlowConfig? ReadLocalConfig(Repository repo)
    {
        try
        {
            var production = repo.Config.Get<string>("gitflow.production")?.Value;
            var development = repo.Config.Get<string>("gitflow.development")?.Value;

            if (production == null || development == null)
                return null;

            return new GitFlowConfig
            {
                ProductionBranch = production,
                DevelopmentBranch = development,
                FeaturePrefix = repo.Config.Get<string>("gitflow.prefix.feature")?.Value ?? "feature/",
                ReleasePrefix = repo.Config.Get<string>("gitflow.prefix.release")?.Value ?? "release/",
                HotfixPrefix = repo.Config.Get<string>("gitflow.prefix.hotfix")?.Value ?? "hotfix/",
                BugfixPrefix = repo.Config.Get<string>("gitflow.prefix.bugfix")?.Value ?? "bugfix/",
                VersionPrefix = repo.Config.Get<string>("gitflow.prefix.version")?.Value ?? "v",
                MergeStrategy = repo.Config.Get<string>("gitflow.merge.strategy")?.Value ?? "--no-ff",
                IsGlobal = false
            };
        }
        catch
        {
            return null;
        }
    }

    public GitFlowConfig? ReadGlobalConfig()
    {
        try
        {
            var production = GetGitConfigValue("gitflow.production", global: true);
            var development = GetGitConfigValue("gitflow.development", global: true);

            if (production == null || development == null)
                return null;

            return new GitFlowConfig
            {
                ProductionBranch = production,
                DevelopmentBranch = development,
                FeaturePrefix = GetGitConfigValue("gitflow.prefix.feature", global: true) ?? "feature/",
                ReleasePrefix = GetGitConfigValue("gitflow.prefix.release", global: true) ?? "release/",
                HotfixPrefix = GetGitConfigValue("gitflow.prefix.hotfix", global: true) ?? "hotfix/",
                BugfixPrefix = GetGitConfigValue("gitflow.prefix.bugfix", global: true) ?? "bugfix/",
                VersionPrefix = GetGitConfigValue("gitflow.prefix.version", global: true) ?? "v",
                MergeStrategy = GetGitConfigValue("gitflow.merge.strategy", global: true) ?? "--no-ff",
                IsGlobal = true
            };
        }
        catch
        {
            return null;
        }
    }

    public bool ConfigExists(bool global = false)
    {
        return ReadConfig(global) != null;
    }

    public void WriteConfig(GitFlowConfig config, bool global = false)
    {
        try
        {
            if (global)
            {
                WriteGlobalConfig(config);
            }
            else
            {
                var repo = GitRepositoryService.GetRepository();
                WriteLocalConfig(repo, config);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write GitFlow configuration: {ex.Message}", ex);
        }
    }

    private void WriteLocalConfig(Repository repo, GitFlowConfig config)
    {
        repo.Config.Set("gitflow.production", config.ProductionBranch);
        repo.Config.Set("gitflow.development", config.DevelopmentBranch);
        repo.Config.Set("gitflow.prefix.feature", config.FeaturePrefix);
        repo.Config.Set("gitflow.prefix.release", config.ReleasePrefix);
        repo.Config.Set("gitflow.prefix.hotfix", config.HotfixPrefix);
        repo.Config.Set("gitflow.prefix.bugfix", config.BugfixPrefix);
        repo.Config.Set("gitflow.prefix.version", config.VersionPrefix);
        repo.Config.Set("gitflow.merge.strategy", config.MergeStrategy);
    }

    private void WriteGlobalConfig(GitFlowConfig config)
    {
        SetGitConfigValue("gitflow.production", config.ProductionBranch, global: true);
        SetGitConfigValue("gitflow.development", config.DevelopmentBranch, global: true);
        SetGitConfigValue("gitflow.prefix.feature", config.FeaturePrefix, global: true);
        SetGitConfigValue("gitflow.prefix.release", config.ReleasePrefix, global: true);
        SetGitConfigValue("gitflow.prefix.hotfix", config.HotfixPrefix, global: true);
        SetGitConfigValue("gitflow.prefix.bugfix", config.BugfixPrefix, global: true);
        SetGitConfigValue("gitflow.prefix.version", config.VersionPrefix, global: true);
        SetGitConfigValue("gitflow.merge.strategy", config.MergeStrategy, global: true);
    }

    public GitFlowConfig GetOrCreateConfig(bool global = false)
    {
        var repo = GitRepositoryService.GetRepository();
        var local = ReadLocalConfig(repo);
        if (local != null)
            return local;

        var globalConfig = ReadGlobalConfig();
        if (globalConfig != null)
            return globalConfig;

        return new GitFlowConfig();
    }

    private string? GetGitConfigValue(string key, bool global = false)
    {
        try
        {
            var args = global ? $"config --global --get {key}" : $"config --get {key}";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            return string.IsNullOrEmpty(output) ? null : output;
        }
        catch
        {
            return null;
        }
    }

    private void SetGitConfigValue(string key, string value, bool global = false)
    {
        var args = global ? $"config --global {key} {value}" : $"config {key} {value}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to set git config {key}");
        }
    }
}
