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

Initialize GitFlow with interactive prompts:

```bash
gitflow init [options]
```

Options:
- `-g, --global` - Store configuration globally instead of locally
- `-f, --force` - Overwrite existing configuration

The command will interactively ask for:
1. Production branch (default: main)
2. Development branch (default: develop)
3. Feature branch prefix (default: feature/)
4. Release branch prefix (default: release/)
5. Hotfix branch prefix (default: hotfix/)
6. Bugfix branch prefix (default: bugfix/)
7. Version tag prefix (default: v)
8. Merge strategy (default: --no-ff)

Example session:
```bash
$ gitflow init

GitFlow Configuration

Production branch [main]: 
Development branch [develop]: 

Branch Prefixes

Feature branch prefix [feature/]: 
Release branch prefix [release/]: 
Hotfix branch prefix [hotfix/]: 
Bugfix branch prefix [bugfix/]: 
Version tag prefix [v]: 

Merge Strategy

Merge strategy [--no-ff]: 

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

To use global configuration:
```bash
gitflow init -g
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

GitFlow configuration is stored in git config (locally or globally).

Local configuration (`.git/config`):
```
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

Global configuration (`~/.gitconfig`):
```bash
gitflow init -g  # Store globally
```

## Requirements

- .NET 10.0
- Git repository
- LibGit2Sharp
- System.CommandLine

## License

MIT