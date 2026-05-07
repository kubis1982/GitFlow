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

Use the `gitflow hooks register` command to automatically install hook templates:

```bash
# For .NET projects
gitflow hooks register dotnet

# For Node.js/npm projects
gitflow hooks register nodejs
```

This command:
- Copies hook templates from `docs/hooks/{template}/` to `.git/hooks/`
- Validates each hook (must contain "version" marker)
- Skips existing hooks (won't overwrite)
- Shows summary of registered/skipped/failed hooks

### Manual Installation

1. Create the `.git/hooks/` directory if it doesn't exist
2. Copy hook scripts from `docs/hooks/` to `.git/hooks/`
3. Customize the hooks for your project needs

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
