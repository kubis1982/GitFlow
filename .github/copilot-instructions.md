# GitFlow - Copilot Instructions

## Build & Run

```bash
# Build the project
dotnet build

# Run locally
dotnet run --project src/GitFlow

# Pack as NuGet package
dotnet pack src/GitFlow/GitFlow.csproj --configuration Release --output ./artifacts

# Publish single-file executable (Windows x64)
dotnet publish src/GitFlow/GitFlow.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
```

The project uses .NET 10.0 with PublishAot enabled for native compilation.

## Architecture Overview

### Command Structure

GitFlow uses a **base class hierarchy** for command operations:

1. **Base Command Classes** (`Commands/Base/`)
   - `StartCommandBase` - Creates new branches from source branch
   - `FinishCommandBase` - Merges branches and cleans up
   - `PublishCommandBase` - Pushes branches to remote
   - `CheckoutCommandBase` - Switches to branches
   - `UpdateCommandBase` - Pulls latest changes
   - `DeleteCommandBase` - Removes branches
   - `ListCommandBase` - Lists branches by type

2. **Concrete Commands** (`Commands/`)
   - Each branch type (Feature, Bugfix, Release, Hotfix) extends base classes
   - Commands define only configuration-specific logic:
     - `GetBranchPrefix(config)` - Returns prefix (e.g., `feature/`, `hotfix/`)
     - `GetSourceBranch(config)` - Returns source branch (develop or main)
     - `PerformFinish(...)` - Custom merge behavior for releases/hotfixes

3. **Configuration Commands**
   - `InitCommand` - Interactive setup with prompts for all settings
   - `ConfigCommand` - Global template configuration with forced defaults

### Service Layer

All git operations go through services (`Services/`):

- **ConfigurationService** - Reads/writes gitflow config in `.git/config` or `~/.gitconfig`
- **GitRepositoryService** - Repository access and validation
- **BranchService** - Branch creation, checkout, deletion, verification
- **MergeService** - Merge operations with strategy support
- **FetchService** - Remote operations (fetch, pull)

### Configuration Model

**GitFlowConfig** (`Models/GitFlowConfig.cs`) stores:
- Production/Development branch names
- Branch prefixes (feature/, release/, hotfix/, bugfix/)
- Version tag prefix (v or empty)
- Merge strategy (--no-ff, --ff, --ff-only)
- IsGlobal flag

Config is stored in git config using keys like:
```
gitflow.production
gitflow.development
gitflow.prefix.feature
gitflow.merge.strategy
```

### Branch Flow Logic

**Feature/Bugfix**: `develop` → `feature/X` → `develop`
- Created from development branch
- Merged back to development only
- Deleted after merge

**Release**: `develop` → `release/X` → `main` + `develop` (tagged)
- Created from development
- Merged to production (creates tag)
- Back-merged to development
- Deleted after merge

**Hotfix**: `main` → `hotfix/X` → `main` + `develop` (tagged)
- Created from production (not develop!)
- Merged to production (creates tag)
- Back-merged to development
- Deleted after merge

## Key Conventions

### Command Registration

Add new commands to `Program.cs`:
```csharp
rootCommand.Add(new YourCommand());
```

### System.CommandLine Patterns

**Creating Options** (two valid syntaxes):
```csharp
// Single alias
var option = new Option<bool>("-f") { Description = "..." };

// Multiple aliases (short + long form)
var option = new Option<bool>("-f", "--force") { Description = "..." };
var option = new Option<bool>(new[] { "-f", "--force" }) { Description = "..." };
```

**Adding Options to Commands**:
```csharp
// Direct add (for simple commands)
Add(forceOption);

// Via Options collection (when adding multiple)
Options.Add(globalOption);
Options.Add(forceOption);

// Explicit method call
AddOption(forceOption);
```

**Setting Command Actions**:
```csharp
SetAction(n =>
{
    var value = n.GetValue(optionOrArgument);
    // command logic
});
```

All three approaches for adding options are valid. The project uses both `Add()` and `Options.Add()` interchangeably.

### Base Class Pattern

When adding a new branch type:
1. Extend appropriate base classes (StartCommandBase, FinishCommandBase, etc.)
2. Override `GetBranchPrefix()` and `GetSourceBranch()`
3. For custom finish logic (like releases), override `PerformFinish()`

Example:
```csharp
internal class FeatureStartCommand : StartCommandBase
{
    public FeatureStartCommand() : base("feature") { }
    protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
    protected override string GetSourceBranch(GitFlowConfig config) => config.DevelopmentBranch;
}
```

### Console Output

Use `ConsoleHelper` for all output:
- `ConsoleHelper.PrintSuccess()` - Green success messages
- `ConsoleHelper.PrintError()` - Red error messages
- `ConsoleHelper.PrintInfo()` - Cyan informational messages

### Configuration Access

Always use `ConfigurationService.GetOrCreateConfig()` which:
1. Checks local repo config first
2. Falls back to global config
3. Returns defaults if nothing configured

### Branch Verification

Before finishing branches:
1. `BranchService.VerifyWorkingBranchIsUpToDate()` - Ensures current branch is synced (blocking)
2. `BranchService.EnsureBranchIsUpToDate()` - Ensures target branch is synced (non-blocking warning)

### Merge Strategy Implementation

Map config values to LibGit2Sharp's `FastForwardStrategy`:
- `--no-ff` → `FastForwardStrategy.NoFastForward`
- `--ff` → (default LibGit2Sharp behavior)
- `--ff-only` → `FastForwardStrategy.FastForwardOnly`

### Interactive Prompts

For configuration commands:
- Show default values in brackets: `[main]`
- Accept empty input as default
- Provide numbered choices for complex options (merge strategy, version prefix)

### Error Handling

All command actions wrapped in try-catch:
```csharp
try {
    // command logic
} catch (Exception ex) {
    ConsoleHelper.PrintError($"Error: {ex.Message}");
}
```

### Git Config Values

Global config uses `git config --global` via Process:
```csharp
SetGitConfigValue("gitflow.production", "main", global: true)
```

Local config uses LibGit2Sharp's Repository.Config:
```csharp
repo.Config.Set("gitflow.production", "main")
```

### Version Tags

Release and Hotfix finish commands create tags:
```csharp
repo.ApplyTag($"{config.VersionPrefix}{version}")
```

Empty VersionPrefix is supported (no "v" prefix).

## Release Process

1. Create version tag: `git tag v1.0.0`
2. Push tag: `git push origin v1.0.0`
3. GitHub Actions workflow (`release.yml`) automatically:
   - Builds the project
   - Packs NuGet package
   - Publishes single-file executable
   - Pushes to NuGet.org
   - Creates GitHub Release with assets

## Project Structure

```
src/GitFlow/
├── Commands/
│   ├── Base/              # Abstract base classes for reusable command logic
│   ├── FeatureCommand.cs  # Feature branch operations
│   ├── BugfixCommand.cs   # Bugfix branch operations
│   ├── ReleaseCommand.cs  # Release branch operations
│   ├── HotfixCommand.cs   # Hotfix branch operations
│   ├── InitCommand.cs     # Interactive initialization
│   └── ConfigCommand.cs   # Global template configuration
├── Services/              # Git operations abstraction
├── Models/                # Configuration models
├── Utilities/             # Console helpers
└── Program.cs             # Command registration
```
