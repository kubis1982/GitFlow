using System.Diagnostics;
using GitFlow.Models;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Services;

public static class HookService
{
    /// <summary>
    /// Gets the path to the docs/hooks directory
    /// First tries the tool installation directory, then falls back to repository root (for development)
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <returns>Full path to docs/hooks directory</returns>
    private static string GetDocsHooksPath(Repository repo)
    {
        // Try tool installation directory first (when installed via NuGet)
        var appDir = AppContext.BaseDirectory;
        
        if (!string.IsNullOrEmpty(appDir))
        {
            var toolHooksPath = Path.Combine(appDir, "docs", "hooks");
            if (Directory.Exists(toolHooksPath))
            {
                return toolHooksPath;
            }
        }
        
        // Fallback to repository root (for development/running from source)
        var repoRoot = repo.Info.WorkingDirectory;
        return Path.Combine(repoRoot, "docs", "hooks");
    }

    /// <summary>
    /// Gets the path to the .git/hooks directory
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <returns>Full path to .git/hooks directory</returns>
    private static string GetGitHooksPath(Repository repo)
    {
        return Path.Combine(repo.Info.Path, "hooks");
    }

    /// <summary>
    /// Gets the full path for a hook file in .git/hooks
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="hookName">Hook filename</param>
    /// <returns>Full path to hook file</returns>
    private static string GetHookPath(Repository repo, string hookName)
    {
        return Path.Combine(GetGitHooksPath(repo), hookName);
    }

    /// <summary>
    /// Gets the subdirectory and hook filenames for a specific template
    /// </summary>
    /// <param name="templateName">Template name: "dotnet" or "nodejs"</param>
    /// <returns>Tuple with subdirectory name and array of hook filenames</returns>
    private static (string subdirectory, string[] hookFiles) GetTemplateFiles(string templateName)
    {
        var hookFiles = new[]
        {
            "gitflow-release-start-post.cs",
            "gitflow-hotfix-start-post.cs"
        };

        if (templateName.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return ("dotnet", hookFiles);
        }

        if (templateName.Equals("nodejs", StringComparison.OrdinalIgnoreCase))
        {
            return ("nodejs", hookFiles);
        }

        throw new ArgumentException($"Unknown template: {templateName}. Available templates: dotnet, nodejs");
    }

    /// <summary>
    /// Validates that a hook file exists and contains the required "version" marker
    /// </summary>
    /// <param name="sourceFilePath">Path to the hook file in docs/hooks</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool ValidateHookFile(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            return false;
        }

        try
        {
            var content = File.ReadAllText(sourceFilePath);
            return content.Contains("version", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Copies a hook file from docs/hooks to .git/hooks
    /// </summary>
    /// <param name="sourceFilePath">Source file path</param>
    /// <param name="targetFilePath">Target file path</param>
    private static void CopyHookFile(string sourceFilePath, string targetFilePath)
    {
        var targetDir = Path.GetDirectoryName(targetFilePath);
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir!);
        }

        File.Copy(sourceFilePath, targetFilePath, overwrite: false);
    }

    /// <summary>
    /// Registers hooks from a template
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="templateName">Template name</param>
    /// <returns>HookRegistrationResult with statistics</returns>
    public static HookRegistrationResult RegisterTemplate(Repository repo, string templateName)
    {
        var result = new HookRegistrationResult();
        var docsHooksPath = GetDocsHooksPath(repo);

        if (!Directory.Exists(docsHooksPath))
        {
            throw new DirectoryNotFoundException($"Hook templates directory not found: {docsHooksPath}");
        }

        var (subdirectory, hookFiles) = GetTemplateFiles(templateName);
        var templatePath = Path.Combine(docsHooksPath, subdirectory);

        if (!Directory.Exists(templatePath))
        {
            throw new DirectoryNotFoundException($"Template directory not found: {templatePath}");
        }

        foreach (var hookFile in hookFiles)
        {
            var sourceFile = Path.Combine(templatePath, hookFile);
            var targetFile = GetHookPath(repo, hookFile);

            // Validate source file
            if (!ValidateHookFile(sourceFile))
            {
                ConsoleHelper.PrintError($"✗ {hookFile}: validation failed (file not found or missing 'version' marker)");
                result.Failed++;
                continue;
            }

            // Check if target already exists
            if (File.Exists(targetFile))
            {
                ConsoleHelper.PrintInfo($"⊘ {hookFile}: already exists, skipping");
                result.Skipped++;
                continue;
            }

            // Copy file
            try
            {
                CopyHookFile(sourceFile, targetFile);
                ConsoleHelper.PrintSuccess($"✓ {hookFile}: registered");
                result.Copied++;
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"✗ {hookFile}: failed to copy ({ex.Message})");
                result.Failed++;
            }
        }

        return result;
    }

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
