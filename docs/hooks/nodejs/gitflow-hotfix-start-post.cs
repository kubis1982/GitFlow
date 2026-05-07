#!/usr/bin/env dotnet
// GitFlow Hotfix Start Post-Hook for package.json
// This hook is executed AFTER a hotfix branch is created
// Usage: Automatically called by gitflow hotfix start <version>
// Args[0] = Full branch name (e.g., "hotfix/1.0.1")

using System.Text.Json;
using System.Text.Json.Nodes;

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
Console.WriteLine($"Updating package.json to version {version}");

// Update package.json
var packageFile = "package.json";
if (!File.Exists(packageFile))
{
    Console.Error.WriteLine($"Error: {packageFile} not found");
    return 1;
}

try
{
    var jsonText = File.ReadAllText(packageFile);
    var jsonNode = JsonNode.Parse(jsonText);
    
    if (jsonNode == null)
    {
        Console.Error.WriteLine($"Error: Invalid JSON in {packageFile}");
        return 1;
    }

    // Update version field
    jsonNode["version"] = version;

    // Write back with indentation
    var options = new JsonSerializerOptions 
    { 
        WriteIndented = true 
    };
    var updatedJson = jsonNode.ToJsonString(options);
    File.WriteAllText(packageFile, updatedJson + Environment.NewLine);

    Console.WriteLine($"✓ Successfully updated version to {version}");
    // Note: GitFlow will automatically commit these changes
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error updating {packageFile}: {ex.Message}");
    return 1;
}
