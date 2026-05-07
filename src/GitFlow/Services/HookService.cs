using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using GitFlow.Models;
using GitFlow.Utilities;
using LibGit2Sharp;

namespace GitFlow.Services;

public static class HookService
{
    private const string GitHubRawBaseUrl = "https://github.com/kubis1982/GitFlow/raw/main/docs/hooks";

    /// <summary>
    /// Gets the URL to download the ZIP file for a specific template from GitHub
    /// </summary>
    /// <param name="templateName">Template name: "dotnet" or "nodejs"</param>
    /// <returns>Full URL to the ZIP file</returns>
    private static string GetTemplateZipUrl(string templateName)
    {
        return $"{GitHubRawBaseUrl}/{templateName}.zip";
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
    /// Registers hooks from a template by downloading ZIP from GitHub
    /// </summary>
    /// <param name="repo">Git repository</param>
    /// <param name="templateName">Template name: "dotnet" or "nodejs"</param>
    /// <param name="overwrite">Whether to overwrite existing hooks</param>
    /// <returns>HookRegistrationResult with statistics</returns>
    public static HookRegistrationResult RegisterTemplate(Repository repo, string templateName, bool overwrite = false)
    {
        var result = new HookRegistrationResult();

        var zipUrl = GetTemplateZipUrl(templateName);
        var hooksPath = GetGitHooksPath(repo);
        
        // Ensure .git/hooks directory exists
        if (!Directory.Exists(hooksPath))
        {
            Directory.CreateDirectory(hooksPath);
        }

        ConsoleHelper.PrintInfo($"Downloading hooks from {zipUrl}...");

        try
        {
            // Download ZIP file
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            var zipBytes = httpClient.GetByteArrayAsync(zipUrl).Result;

            // Extract to temporary directory first
            var tempDir = Path.Combine(Path.GetTempPath(), $"gitflow-hooks-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                var tempZipPath = Path.Combine(tempDir, $"{templateName}.zip");
                File.WriteAllBytes(tempZipPath, zipBytes);

                // Extract ZIP
                ZipFile.ExtractToDirectory(tempZipPath, tempDir, overwriteFiles: true);

                // Copy hook files to .git/hooks
                var extractedFiles = Directory.GetFiles(tempDir, "*.cs", SearchOption.AllDirectories);
                
                foreach (var sourceFile in extractedFiles)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var targetFile = Path.Combine(hooksPath, fileName);

                    // Check if target already exists and we're not forcing overwrite
                    if (File.Exists(targetFile) && !overwrite)
                    {
                        ConsoleHelper.PrintInfo($"⊘ {fileName}: already exists, skipping");
                        result.Skipped++;
                        continue;
                    }

                    // Copy file
                    try
                    {
                        File.Copy(sourceFile, targetFile, overwrite: overwrite);
                        ConsoleHelper.PrintSuccess($"✓ {fileName}: applied");
                        result.Copied++;
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.PrintError($"✗ {fileName}: failed to copy ({ex.Message})");
                        result.Failed++;
                    }
                }
            }
            finally
            {
                // Clean up temporary directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to download hooks from GitHub: {ex.Message}. Please check your internet connection.", ex);
        }
        catch (TaskCanceledException)
        {
            throw new Exception("Download timeout. Please check your internet connection and try again.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to register hooks: {ex.Message}", ex);
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
