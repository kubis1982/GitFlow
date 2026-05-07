# GitFlow Hooks

GitFlow supports C# hooks that can be executed at various stages of the workflow. Hooks are `.cs` scripts located in the `.git/hooks/` directory and executed using `dotnet [script].cs`.

## Available Hooks

### Release Hooks
- **gitflow-release-start-pre.cs** - Executed BEFORE creating a release branch
- **gitflow-release-start-post.cs** - Executed AFTER creating a release branch

### Hotfix Hooks
- **gitflow-hotfix-start-pre.cs** - Executed BEFORE creating a hotfix branch
- **gitflow-hotfix-start-post.cs** - Executed AFTER creating a hotfix branch

### Feature Hooks
- **gitflow-feature-start-pre.cs** - Executed BEFORE creating a feature branch
- **gitflow-feature-start-post.cs** - Executed AFTER creating a feature branch

### Bugfix Hooks
- **gitflow-bugfix-start-pre.cs** - Executed BEFORE creating a bugfix branch
- **gitflow-bugfix-start-post.cs** - Executed AFTER creating a bugfix branch

## Hook Parameters

All hooks receive one argument:
- **args[0]** - Full branch name (e.g., `release/1.0.0`, `hotfix/1.0.1`)

To extract the version from the branch name:
```csharp
var version = branchName.Substring(branchName.LastIndexOf('/') + 1);
```

## Hook Execution

Hooks are executed by GitFlow using:
```bash
dotnet .git/hooks/[hookname].cs [branch-name]
```

### Exit Codes
- **0** - Success (operation continues)
- **Non-zero** - Failure (operation aborts)

### Output
- **stdout** - Displayed to user
- **stderr** - Displayed to user as error

### Automatic Commit
**POST hooks only**: After a successful POST hook execution, GitFlow automatically commits any file changes made by the hook. You don't need to manually run git commands in your hook.

## Installing Hook Templates

GitFlow provides pre-built hook templates that can be installed with a single command:

```bash
# For .NET projects (updates Directory.Build.props)
gitflow hooks register dotnet

# For Node.js/npm projects (updates package.json)
gitflow hooks register nodejs
```

This command:
- Copies hook templates from `docs/hooks/{template}/` to `.git/hooks/`
- Validates each hook (must contain "version" marker)
- Skips existing hooks (won't overwrite)
- Shows summary of registered/skipped/failed hooks

### Available Templates

**dotnet** - For .NET projects
- Source: `docs/hooks/dotnet/`
- Target: `.git/hooks/gitflow-*-start-post.cs`
- Updates version in Directory.Build.props

**nodejs** - For Node.js/npm projects
- Source: `docs/hooks/nodejs/`
- Target: `.git/hooks/gitflow-*-start-post.cs`
- Updates version in package.json

**Note**: Both templates install hooks with the same filenames (`gitflow-release-start-post.cs` and `gitflow-hotfix-start-post.cs`) but with different implementations. Choose the template that matches your project type.


Commit message format: `chore: apply {hookname} changes for {branchname}`

Example:
```
chore: apply gitflow-release-start-post.cs changes for release/1.0.0
```

## Installation

### Using Templates (Recommended)

Use the `gitflow hooks register` command to automatically install hook templates:

```bash
# For .NET projects
gitflow hooks register dotnet

# For Node.js/npm projects
gitflow hooks register nodejs
```

This command copies the appropriate hook files from `docs/hooks/{template}/` to `.git/hooks/` with the correct names.

### Manual Installation

1. Create the `.git/hooks/` directory if it doesn't exist
2. Copy hook scripts from `docs/hooks/` to `.git/hooks/`
3. Rename files to match the expected hook names:
   - `gitflow-*-dotnet.cs` → `gitflow-*.cs` (for .NET projects)
   - `gitflow-*-nodejs.cs` → `gitflow-*.cs` (for Node.js projects)
3. Customize the hooks for your project needs

## Example: Update Version in Directory.Build.props

The provided hooks automatically update the `<Version>` tag in `Directory.Build.props` when creating release or hotfix branches.

**Note**: Release and hotfix hooks are provided as examples in `docs/hooks/`. Feature and bugfix hooks follow the same pattern but are not included as examples since version updates are typically not needed for these branch types. All file changes made by POST hooks are automatically committed by GitFlow.

**gitflow-release-start-post.cs**:
```csharp
#!/usr/bin/env dotnet

var branchName = args[0]; // e.g., "release/1.0.0"
var version = branchName.Substring(branchName.LastIndexOf('/') + 1); // "1.0.0"

var propsFile = "Directory.Build.props";
if (!File.Exists(propsFile))
{
    Console.Error.WriteLine($"Error: {propsFile} not found");
    return 1;
}

var content = File.ReadAllText(propsFile);
var updated = System.Text.RegularExpressions.Regex.Replace(
    content, 
    @"<Version>.*?</Version>", 
    $"<Version>{version}</Version>"
);

File.WriteAllText(propsFile, updated);
Console.WriteLine($"✓ Successfully updated version to {version}");
// Note: No need to commit - GitFlow will automatically commit POST hook changes
return 0;
```

## Example: Update Version in package.json

For Node.js/npm projects, hooks can update the `version` field in `package.json`:

**gitflow-release-start-post-packagejson.cs**:
```csharp
#!/usr/bin/env dotnet

using System.Text.Json;
using System.Text.Json.Nodes;

var branchName = args[0]; // e.g., "release/1.0.0"
var version = branchName.Substring(branchName.LastIndexOf('/') + 1); // "1.0.0"

var packageFile = "package.json";
if (!File.Exists(packageFile))
{
    Console.Error.WriteLine($"Error: {packageFile} not found");
    return 1;
}

var jsonText = File.ReadAllText(packageFile);
var jsonNode = JsonNode.Parse(jsonText);
jsonNode["version"] = version;

var options = new JsonSerializerOptions { WriteIndented = true };
var updatedJson = jsonNode.ToJsonString(options);
File.WriteAllText(packageFile, updatedJson + Environment.NewLine);

Console.WriteLine($"✓ Successfully updated version to {version}");
// Note: GitFlow will automatically commit these changes
return 0;
```

## Custom Hooks

You can create custom hooks for your specific needs:

### Example: Validate Version Format
```csharp
#!/usr/bin/env dotnet
// gitflow-release-start-pre.cs

var branchName = args[0];
var version = branchName.Substring(branchName.LastIndexOf('/') + 1);

// Validate semantic versioning (X.Y.Z)
if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
{
    Console.Error.WriteLine($"Error: Invalid version format '{version}'. Expected format: X.Y.Z");
    return 1;
}

Console.WriteLine($"✓ Version format valid: {version}");
return 0;
```

### Example: Update CHANGELOG.md
```csharp
#!/usr/bin/env dotnet
// gitflow-release-start-post.cs

var branchName = args[0];
var version = branchName.Substring(branchName.LastIndexOf('/') + 1);

var changelogFile = "CHANGELOG.md";
if (File.Exists(changelogFile))
{
    var content = File.ReadAllText(changelogFile);
    var newEntry = $"\n## [{version}] - {DateTime.Now:yyyy-MM-dd}\n\n### Added\n- \n\n### Changed\n- \n\n### Fixed\n- \n";
    
    var updated = content.Replace("# Changelog", $"# Changelog{newEntry}");
    File.WriteAllText(changelogFile, updated);
    Console.WriteLine($"✓ Added version {version} to CHANGELOG.md");
}

return 0;
```

### Example: Notify Team via Webhook
```csharp
#!/usr/bin/env dotnet
// gitflow-release-start-post.cs

using System.Net.Http;
using System.Text;
using System.Text.Json;

var branchName = args[0];
var version = branchName.Substring(branchName.LastIndexOf('/') + 1);

var webhookUrl = Environment.GetEnvironmentVariable("TEAM_WEBHOOK_URL");
if (!string.IsNullOrEmpty(webhookUrl))
{
    using var client = new HttpClient();
    var message = new { text = $"🚀 New release branch created: {version}" };
    var json = JsonSerializer.Serialize(message);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    await client.PostAsync(webhookUrl, content);
    Console.WriteLine("✓ Team notified");
}

return 0;
```

### Example: Feature Branch Validation
```csharp
#!/usr/bin/env dotnet
// gitflow-feature-start-pre.cs

var branchName = args[0];
var featureName = branchName.Substring(branchName.LastIndexOf('/') + 1);

// Ensure feature name follows JIRA ticket format (e.g., PROJ-123)
if (!System.Text.RegularExpressions.Regex.IsMatch(featureName, @"^[A-Z]+-\d+$"))
{
    Console.Error.WriteLine($"Error: Feature name '{featureName}' must follow JIRA format (e.g., PROJ-123)");
    return 1;
}

Console.WriteLine($"✓ Feature name valid: {featureName}");
return 0;
```

### Example: Create Feature Branch README
```csharp
#!/usr/bin/env dotnet
// gitflow-feature-start-post.cs

var branchName = args[0];
var featureName = branchName.Substring(branchName.LastIndexOf('/') + 1);

// Create a feature-specific README
var readmeFile = $"docs/features/{featureName}.md";
var directory = Path.GetDirectoryName(readmeFile);

if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

var content = $@"# Feature: {featureName}

## Description
[Add feature description here]

## Implementation Details
- 

## Testing
- 

## Related Issues
- 
";

File.WriteAllText(readmeFile, content);
Console.WriteLine($"✓ Created feature README: {readmeFile}");
return 0;
```

## Requirements

- **.NET 10.0+** - Required for running C# scripts directly with `dotnet [file].cs`
- **GitFlow** - Must be initialized in the repository (`gitflow config init`)

## Troubleshooting

### Hook not executing
- Verify the hook file exists in `.git/hooks/`
- Check the hook filename matches the expected pattern
- Ensure .NET 10+ is installed (`dotnet --version`)

### Hook fails silently
- Check hook script for syntax errors
- Add error handling and output to stdout/stderr
- Test hook manually: `dotnet .git/hooks/[hookname].cs "test/1.0.0"`

### Operation aborted
- Hook returned non-zero exit code
- Check stderr output for error details
- Fix the issue and try the operation again

## Notes

- Hooks are **optional** - GitFlow works without them
- Hooks are **repository-specific** (stored in `.git/hooks/`)
- Hooks are **not committed** to version control (`.git/` is ignored)
- Team members must install hooks individually
- Consider documenting required hooks in your project README
