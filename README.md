# GitFlow

GitFlow - Git workflow management tool implementing the GitFlow branching model.

## Installation

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

```bash
gitflow init [options]
```

Options:
- `-g, --global` - Store configuration globally instead of locally
- `-f, --force` - Overwrite existing configuration
- `--production <branch>` - Production branch name (default: main)
- `--development <branch>` - Development branch name (default: develop)
- `--feature-prefix <prefix>` - Feature branch prefix (default: feature/)
- `--release-prefix <prefix>` - Release branch prefix (default: release/)
- `--hotfix-prefix <prefix>` - Hotfix branch prefix (default: hotfix/)
- `--bugfix-prefix <prefix>` - Bugfix branch prefix (default: bugfix/)
- `--version-prefix <prefix>` - Version tag prefix (default: v)
- `--merge-strategy <strategy>` - Merge strategy (default: --no-ff)

Example:
```bash
gitflow init --production main --development develop
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