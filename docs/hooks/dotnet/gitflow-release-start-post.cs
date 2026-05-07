#!/usr/bin/env dotnet
// GitFlow Release Start Post-Hook
// This hook is executed AFTER a release branch is created
// Usage: Automatically called by gitflow release start <version>
// Args[0] = Full branch name (e.g., "release/1.0.0")

var branchName = args.Length > 0 ? args[0] : null;
if (string.IsNullOrEmpty(branchName))
{
    Console.Error.WriteLine("Error: No branch name provided");
    return 1;
}

// Extract version from branch name (everything after last slash)
var lastSlashIndex = branchName.LastIndexOf('/');
if (lastSlashIndex < 0)
{
    Console.Error.WriteLine($"Error: Invalid branch name format: {branchName}");
    return 1;
}
var version = branchName.Substring(lastSlashIndex + 1);
Console.WriteLine($"Updating Directory.Build.props to version {version}");

// Update Directory.Build.props
var propsFile = "Directory.Build.props";
if (!File.Exists(propsFile))
{
    Console.Error.WriteLine($"Error: {propsFile} not found");
    return 1;
}

try
{
    var content = File.ReadAllText(propsFile);
    var updated = System.Text.RegularExpressions.Regex.Replace(
        content, 
        @"<Version>.*?</Version>", 
        $"<Version>{version}</Version>"
    );

    File.WriteAllText(propsFile, updated);
    Console.WriteLine($"✓ Successfully updated version to {version}");
    // Note: GitFlow will automatically commit these changes
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error updating {propsFile}: {ex.Message}");
    return 1;
}
