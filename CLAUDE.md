# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

GitFlow is a .NET global tool (`gitflow` CLI) that implements the GitFlow branching model on top of a real git repository via LibGit2Sharp. It has no test suite and no build/lint scripts beyond the standard `dotnet` CLI.

## Commands

```bash
# Build
dotnet build

# Run locally against the current directory's git repo
dotnet run --project src/GitFlow -- <args>          # e.g. -- config init

# Pack as a NuGet global tool package
dotnet pack src/GitFlow/GitFlow.csproj --configuration Release --output ./artifacts

# Publish a single-file, AOT-compiled executable (Windows x64)
dotnet publish src/GitFlow/GitFlow.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
```

There is no test project in this repo (`find . -iname "*test*"` returns nothing) and CI does not run tests — verification is manual (`dotnet build` + exercising commands against a scratch git repo).

Target framework is `net10.0` with `PublishAot=true` — keep code AOT/trimming-friendly (avoid reflection-heavy patterns) when touching command/argument wiring.

## Architecture

### Entry point and command tree

`src/GitFlow/Program.cs` builds a `System.CommandLine` `RootCommand` and registers one top-level `Command` per branch type (`ConfigCommand`, `FeatureCommand`, `BugfixCommand`, `ReleaseCommand`, `HotfixCommand`, `HooksCommand`). Adding a new top-level command means adding one line here.

### Base-class-per-verb pattern

`src/GitFlow/Commands/Base/` defines one abstract base class per *verb* shared across branch types: `StartCommandBase`, `PublishCommandBase`, `CheckoutCommandBase`, `UpdateCommandBase`, `DeleteCommandBase`, `ListCommandBase`, `FinishCommandBase`. Each concrete branch command (`FeatureCommand.cs`, `BugfixCommand.cs`, `ReleaseCommand.cs`, `HotfixCommand.cs`) subclasses these and overrides only branch-specific hooks, typically just:

```csharp
protected override string GetBranchPrefix(GitFlowConfig config) => config.FeaturePrefix;
protected override string GetSourceBranch(GitFlowConfig config) => config.DevelopmentBranch;
```

`FinishCommandBase` is the one exception with real per-branch logic: it delegates to an overridden `PerformFinish(...)`. Feature/Bugfix inline their (simple) finish logic directly in the command class; Release/Hotfix delegate to `MergeService.FinishRelease` / `MergeService.FinishHotfix` because those flows are more involved (production merge + tag + dev back-merge). When adding a new branch type, follow this same split: keep single-target merges inline, move multi-target/tagged merges into `MergeService`.

### Service layer

All git and I/O side effects live in `src/GitFlow/Services/`, never directly in command classes:
- `GitRepositoryService` — repo discovery/validation (`IsGitRepository`, `GetRepository`)
- `ConfigurationService` — reads/writes `gitflow.*` keys via LibGit2Sharp's `repo.Config`, at `ConfigurationLevel.Local` (this repo's `.git/config`) or `ConfigurationLevel.Global` (`~/.gitconfig`, used as a template for `config init`)
- `BranchService` — branch existence/creation/deletion, up-to-date verification
- `MergeService` — the multi-step release/hotfix finish flows (merge → tag → push → back-merge → delete → push)
- `FetchService` — remote fetch/pull
- `HookService` — discovers, executes, and downloads C# hook scripts (see below)

### Configuration model

`GitFlowConfig` (`Models/GitFlowConfig.cs`) is the in-memory shape of `gitflow.*` git-config keys (production/development branch names, four branch prefixes, version tag prefix, merge strategy, `IsGlobal`). Local config always takes precedence over the global template; the global template only supplies defaults during interactive `config init`. Every command's `SetAction` follows the same guard sequence: confirm it's a git repo → `ConfigurationService.ReadConfig(false)` → bail out with a `ConsoleHelper.PrintError` if local config is missing (commands never fall back to the global template at runtime, only `config init` does).

### Branch flow semantics

- **Feature/Bugfix**: `develop` → `feature|bugfix/X` → merge back to `develop` only, no tag.
- **Release**: `develop` → `release/X` → merge to `main` (create version tag) → back-merge to `develop`.
- **Hotfix**: `main` (not `develop`!) → `hotfix/X` → merge to `main` (create version tag) → back-merge to `develop`.

Version tags use `config.VersionPrefix + version`, where `version` is the branch name with its prefix stripped (e.g. `release/1.0.0` → tag `v1.0.0`). An empty `VersionPrefix` is valid.

### Hooks

Hooks are optional user-authored C# scripts (`gitflow-<branchType>-start-pre.cs` / `-post.cs`) that live in `.git/hooks/` (not versioned) and run via `dotnet <script.cs> <branchName>` (`HookService.ExecuteHook`). Pre-hooks can abort branch creation by failing; post-hook file changes are auto-staged and committed by `HookService.CommitHookChanges`. `gitflow hooks apply <dotnet|nodejs>` downloads a template ZIP from `docs/hooks/*.zip` on GitHub's raw content (`HookService.RegisterTemplate`) and extracts it into `.git/hooks/` — this requires network access and is the reason `HookService` exists as a separate concern from `BranchService`.

### Console output

All user-facing output goes through `Utilities/ConsoleHelper` (`PrintSuccess`/`PrintError`/`PrintInfo`/`PrintWarning`) rather than raw `Console.Write*`, for consistent coloring.

### Versioning and release

`Directory.Build.props` holds the single source-of-truth `<Version>` for the whole solution. Two independent GitHub Actions workflows consume it differently:
- `release.yml` triggers on `v*.*.*` tag push, packs/publishes/pushes to NuGet.org and creates a GitHub Release with a win-x64 zip.
- `prerelease.yml` is manually dispatched and derives a prerelease version as `<Directory.Build.props version>-dev.<timestamp>+<shortSha>` without needing a tag.

Note `.github/copilot-instructions.md` predates the current `Commands/ConfigCommand.cs` structure (it still refers to a standalone `InitCommand.cs`; `InitCommand`/`ShowCommand`/`TemplateCommand` are now nested private classes inside `ConfigCommand`) — trust the code over that file for command layout, but its System.CommandLine usage notes and try/catch conventions are still accurate.
