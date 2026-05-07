using System.Diagnostics;
using GitFlow.Models;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Services;

public static class HookService
{
    /// <summary>
    /// Checks if a hook file exists in the .git/hooks/ directory
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="hookName">Hook filename (e.g., "gitflow-release-start-pre.cs")</param>
    /// <returns>True if hook exists, false otherwise</returns>
    public static bool HookExists(Repository repo, string hookName)
    {
        var hooksPath = Path.Combine(repo.Info.Path, "hooks");
        var hookFilePath = Path.Combine(hooksPath, hookName);
        return File.Exists(hookFilePath);
    }

    /// <summary>
    /// Executes a hook script using dotnet
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="hookName">Hook filename (e.g., "gitflow-release-start-pre.cs")</param>
    /// <param name="branchName">Full branch name to pass as argument (e.g., "release/1.0.0")</param>
    /// <returns>HookResult containing exit code and output, or null if hook doesn't exist</returns>
    public static HookResult? ExecuteHook(Repository repo, string hookName, string branchName)
    {
        var hooksPath = Path.Combine(repo.Info.Path, "hooks");
        var hookFilePath = Path.Combine(hooksPath, hookName);

        if (!File.Exists(hookFilePath))
        {
            // Hook is optional - return null if it doesn't exist
            return null;
        }

        ConsoleHelper.PrintInfo($"Executing hook: {hookName}");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{hookFilePath}\" \"{branchName}\"",
                WorkingDirectory = repo.Info.WorkingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new System.Text.StringBuilder();
        var error = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                Console.WriteLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
                Console.Error.WriteLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var result = new HookResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = output.ToString(),
            StandardError = error.ToString()
        };

        if (result.Success)
        {
            ConsoleHelper.PrintSuccess($"Hook {hookName} completed successfully");
        }
        else
        {
            ConsoleHelper.PrintError($"Hook {hookName} failed with exit code {result.ExitCode}");
        }

        return result;
    }

    /// <summary>
    /// Commits any changes made by a hook
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="hookName">Hook name for commit message</param>
    /// <param name="branchName">Branch name for commit message</param>
    public static void CommitHookChanges(Repository repo, string hookName, string branchName)
    {
        // Check if there are any uncommitted changes
        var status = repo.RetrieveStatus();
        var hasChanges = status.IsDirty;

        if (!hasChanges)
        {
            // No changes to commit
            return;
        }

        ConsoleHelper.PrintInfo("Committing hook changes...");

        // Stage all changes
        LibGit2Sharp.Commands.Stage(repo, "*");

        // Create commit
        var signature = new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now);
        var message = $"Update files for {branchName}";
        
        repo.Commit(message, signature, signature);
        
        ConsoleHelper.PrintSuccess("Changes committed");
    }
}
