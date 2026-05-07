# GitFlow

GitFlow - Git workflow management tool implementing the GitFlow branching model.

## Installation

### Install as .NET Tool (Recommended)

Install globally using .NET CLI:
```bash
dotnet tool install --global Kubis1982.GitFlow
```

Update to the latest version:
```bash
dotnet tool update --global Kubis1982.GitFlow
```

Uninstall:
```bash
dotnet tool uninstall --global Kubis1982.GitFlow
```

### Build from Source

Build the project:
```bash
dotnet build
```

Run the tool:
```bash
dotnet run --project src/GitFlow
```

## Usage

### Initialize GitFlow

GitFlow must be initialized in each repository before use. The initialization process has three scenarios:

#### Scenario 1: Initialize with Global Template

If you have a global template configured, initialize with defaults:

```bash
gitflow config init
```

This displays the global template configuration and asks for confirmation:
- Answer `Y` to accept all settings
- Answer `n` to customize individual settings

#### Scenario 2: Initialize from Scratch

If no global template exists, configure all settings interactively:

```bash
gitflow config init
```

You'll be prompted for:
1. Production branch (default: main)
2. Development branch (default: develop)
3. Feature branch prefix (default: feature/)
4. Release branch prefix (default: release/)
5. Hotfix branch prefix (default: hotfix/)
6. Bugfix branch prefix (default: bugfix/)
7. Version tag prefix (default: v or none)
8. Merge strategy (default: --no-ff)

#### Scenario 3: Reconfigure Existing Setup

To override existing local configuration:

```bash
gitflow config init -f
```

Options:
- `-f, --force` - Overwrite existing local configuration

Example session:
```bash
$ gitflow config init

GitFlow Configuration

Production branch [main]: 
Development branch [develop]: 

Branch Prefixes

Feature branch prefix [feature/]: 
Release branch prefix [release/]: 
Hotfix branch prefix [hotfix/]: 
Bugfix branch prefix [bugfix/]: 

Version Prefix

Select version tag prefix:
  1) <none> (e.g., 1.0.0)
  2) v (e.g., v1.0.0)
Choice [1]: 2

Merge Strategy

Select merge strategy:
  1) --no-ff (recommended - always create merge commit)
  2) --ff (fast-forward if possible)
  3) --ff-only (only fast-forward, fail otherwise)
Choice [1]: 

✓ GitFlow initialized (local config)
  Production branch: main
  Development branch: develop
  Feature prefix: feature/
  Release prefix: release/
  Hotfix prefix: hotfix/
  Bugfix prefix: bugfix/
  Version prefix: v
  Merge strategy: --no-ff
```

### Configure Global Template

Create a global configuration template for reuse across repositories:

```bash
gitflow config template
```

This configures:
1. Production branch (default: main)
2. Development branch (default: develop)
3. Version tag prefix (v or none)
4. Merge strategy (--no-ff, --ff, or --ff-only)

**Note**: Branch prefixes are fixed as `feature/`, `release/`, `hotfix/`, `bugfix/` in the template.

To override existing global template:
```bash
gitflow config template -f
```

### Feature Branches

Features are created from the development branch.

```bash
# Start a new feature
gitflow feature start <name>

# Publish feature to remote
gitflow feature publish <name>

# Checkout feature branch
gitflow feature checkout <name>

# Update feature with latest development changes
gitflow feature update <name>

# Finish feature (merge to development and delete)
gitflow feature finish <name>

# Delete feature branch
gitflow feature delete <name>

# List all feature branches
gitflow feature list
```

Example:
```bash
gitflow feature start PIQ-234
gitflow feature publish PIQ-234
gitflow feature finish PIQ-234
```

### Bugfix Branches

Bugfixes work the same as features (created from development).

```bash
gitflow bugfix start <name>
gitflow bugfix publish <name>
gitflow bugfix checkout <name>
gitflow bugfix update <name>
gitflow bugfix finish <name>
gitflow bugfix delete <name>
gitflow bugfix list
```

### Release Branches

Releases are created from development and merged to both production and development.

```bash
# Start a new release
gitflow release start <version>

# Publish release to remote
gitflow release publish <version>

# Checkout release branch
gitflow release checkout <version>

# Update release with latest development changes
gitflow release update <version>

# Finish release (merge to production and development, create tag, delete branch)
gitflow release finish <version>

# Delete release branch
gitflow release delete <version>

# List all release branches
gitflow release list
```

Example:
```bash
gitflow release start 1.0.0
gitflow release finish 1.0.0  # Creates tag v1.0.0
```

### Hotfix Branches

Hotfixes are created from production (not development!) and merged to both production and development.

```bash
# Start a new hotfix (from production)
gitflow hotfix start <version>

# Publish hotfix to remote
gitflow hotfix publish <version>

# Checkout hotfix branch
gitflow hotfix checkout <version>

# Update hotfix with latest production changes
gitflow hotfix update <version>

# Finish hotfix (merge to production and development, create tag, delete branch)
gitflow hotfix finish <version>

# Delete hotfix branch
gitflow hotfix delete <version>

# List all hotfix branches
gitflow hotfix list
```

Example:
```bash
gitflow hotfix start 1.0.1
gitflow hotfix finish 1.0.1  # Creates tag v1.0.1
```

## GitFlow Model

### Branch Types

- **Production** (main): Production-ready code
- **Development** (develop): Integration branch for features
- **Feature** (feature/*): New features for upcoming releases
- **Bugfix** (bugfix/*): Bug fixes for upcoming releases
- **Release** (release/*): Preparation for production release
- **Hotfix** (hotfix/*): Emergency fixes for production

### Workflow

1. **Features/Bugfixes**: Develop → Feature → Develop
2. **Releases**: Develop → Release → Production + Development (with tag)
3. **Hotfixes**: Production → Hotfix → Production + Development (with tag)

## Configuration

GitFlow requires local configuration in each repository. Configuration is stored in git config.

### Local Configuration

Local configuration is stored in `.git/config`:
```ini
[gitflow]
    production = main
    development = develop
    prefix.feature = feature/
    prefix.release = release/
    prefix.hotfix = hotfix/
    prefix.bugfix = bugfix/
    prefix.version = v
    merge.strategy = --no-ff
```

**Important**: All GitFlow commands (start, finish, publish, etc.) require local configuration. Run `gitflow config init` first.

### Global Template

Global template is stored in `~/.gitconfig` and used as defaults during `gitflow config init`:
```bash
gitflow config template  # Create global template
```

The template simplifies initialization across multiple repositories by providing default values.

### Configuration Priority

1. **Local** configuration (`.git/config`) - used by all GitFlow commands
2. **Global** template (`~/.gitconfig`) - used as defaults during initialization

## Hooks

GitFlow supports C# hooks that can be executed at various stages of the workflow. Hooks are optional scripts that automate custom actions during GitFlow operations.

### Available Hooks

#### Release Hooks
- **gitflow-release-start-pre.cs** - Executed BEFORE creating a release branch
- **gitflow-release-start-post.cs** - Executed AFTER creating a release branch

#### Hotfix Hooks
- **gitflow-hotfix-start-pre.cs** - Executed BEFORE creating a hotfix branch
- **gitflow-hotfix-start-post.cs** - Executed AFTER creating a hotfix branch

#### Feature & Bugfix Hooks
- **gitflow-feature-start-pre.cs** / **gitflow-feature-start-post.cs** - Feature branch hooks
- **gitflow-bugfix-start-pre.cs** / **gitflow-bugfix-start-post.cs** - Bugfix branch hooks

All hook types follow the same pattern and receive the full branch name as argument.

### Hook Installation

1. Create `.git/hooks/` directory if it doesn't exist
2. Copy hook scripts from `docs/hooks/` to `.git/hooks/`
3. Customize for your project needs

Example:
```bash
# Copy example hooks
cp docs/hooks/gitflow-release-start-post.cs .git/hooks/
cp docs/hooks/gitflow-hotfix-start-post.cs .git/hooks/
```

### Example: Auto-update Version

The provided example hooks automatically update `Directory.Build.props` when creating release or hotfix branches:

```csharp
#!/usr/bin/env dotnet
var branchName = args[0]; // e.g., "release/1.0.0"
var version = branchName.Substring(branchName.LastIndexOf('/') + 1);

var content = File.ReadAllText("Directory.Build.props");
var updated = System.Text.RegularExpressions.Regex.Replace(
    content, 
    @"<Version>.*?</Version>", 
    $"<Version>{version}</Version>"
);
File.WriteAllText("Directory.Build.props", updated);
return 0;
```

### Hook Behavior

- **Parameters**: Hooks receive the full branch name (e.g., `release/1.0.0`)
- **Exit codes**: Return 0 for success, non-zero to abort operation
- **Optional**: Hooks are optional - GitFlow works without them
- **Per-repository**: Hooks are stored in `.git/hooks/` and not committed to version control

For complete hook documentation and examples, see [docs/hooks/README.md](docs/hooks/README.md).

## Requirements

- .NET 10.0
- Git repository
- LibGit2Sharp
- System.CommandLine

## License

MIT