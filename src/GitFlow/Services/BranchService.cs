using LibGit2Sharp;
using GitFlow.Models;
using GitFlow.Utilities;

namespace GitFlow.Services;

public static class BranchService
{
    public static void CreateBranch(Repository repo, string branchName, string sourceBranchName)
    {
        var sourceBranch = repo.Branches[sourceBranchName];
        if (sourceBranch == null)
            throw new InvalidOperationException($"Source branch '{sourceBranchName}' not found");

        var newBranch = repo.CreateBranch(branchName, sourceBranch.Tip);
        LibGit2Sharp.Commands.Checkout(repo, newBranch);
    }

    public static void PublishBranch(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");

        var remote = repo.Network.Remotes["origin"];
        if (remote == null)
            throw new InvalidOperationException("No 'origin' remote configured. Add a remote repository first.");

        // Push branch to remote
        repo.Network.Push(remote, $"refs/heads/{branchName}:refs/heads/{branchName}");
        
        // Set up tracking so pull/push work without specifying remote
        var remoteBranchName = $"refs/remotes/origin/{branchName}";
        repo.Branches.Update(branch, b => b.TrackedBranch = remoteBranchName);
    }

    public static void CheckoutBranch(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        
        // If branch doesn't exist locally, try to fetch from remote
        if (branch == null && FetchService.HasRemote(repo))
        {
            try
            {
                // Fetch from remote to get latest branch information
                FetchService.FetchAll(repo);
                
                // Check if branch exists on remote
                var remoteBranchName = $"origin/{branchName}";
                var remoteBranch = repo.Branches[remoteBranchName];
                
                if (remoteBranch != null)
                {
                    // Create local tracking branch from remote
                    branch = repo.CreateBranch(branchName, remoteBranch.Tip);
                    repo.Branches.Update(branch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                }
            }
            catch (InvalidOperationException)
            {
                // Network error or other fetch issues - rethrow with context
                throw;
            }
        }
        
        if (branch == null)
        {
            var message = FetchService.HasRemote(repo)
                ? $"Branch '{branchName}' not found locally or on remote"
                : $"Branch '{branchName}' not found";
            throw new InvalidOperationException(message);
        }

        LibGit2Sharp.Commands.Checkout(repo, branch);

        // Pull latest changes if tracking remote
        if (branch.IsTracking)
        {
            var options = new PullOptions
            {
                FetchOptions = new FetchOptions()
            };
            LibGit2Sharp.Commands.Pull(repo, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), options);
        }
    }

    public static void DeleteBranch(Repository repo, string branchName, bool deleteRemote = true)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");

        repo.Branches.Remove(branchName);

        if (deleteRemote)
        {
            try
            {
                var remote = repo.Network.Remotes["origin"];
                if (remote != null)
                {
                    repo.Network.Push(remote, $":refs/heads/{branchName}");
                }
            }
            catch
            {
                // Remote might not exist
            }
        }
    }

    public static void UpdateBranch(Repository repo, string branchName, string parentBranchName)
    {
        var branch = repo.Branches[branchName];
        var parentBranch = repo.Branches[parentBranchName];

        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");
        if (parentBranch == null)
            throw new InvalidOperationException($"Parent branch '{parentBranchName}' not found");

        // Check if parent branch is tracking a remote and fetch if needed
        if (parentBranch.IsTracking && FetchService.HasRemote(repo))
        {
            var remoteTip = parentBranch.TrackedBranch?.Tip;
            var localTip = parentBranch.Tip;
            
            // Check if remote is ahead (staleness detection)
            bool shouldFetch = remoteTip == null || remoteTip.Sha != localTip.Sha;
            
            if (shouldFetch)
            {
                try
                {
                    // Fetch the parent branch from remote
                    FetchService.FetchAll(repo);
                    
                    // Refresh parent branch reference after fetch
                    parentBranch = repo.Branches[parentBranchName];
                    
                    ConsoleHelper.PrintInfo($"Fetched latest changes from remote for '{parentBranchName}'");
                }
                catch (InvalidOperationException ex)
                {
                    // Network error - continue with local data
                    ConsoleHelper.PrintWarning($"Could not fetch from remote: {ex.Message}. Using local data.");
                }
            }
        }

        LibGit2Sharp.Commands.Checkout(repo, branch);

        var mergeOptions = new MergeOptions();
        var result = repo.Merge(parentBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), mergeOptions);

        if (result.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException("Merge conflicts detected. Resolve them manually.");
        }
    }

    public static List<BranchInfo> ListBranches(Repository repo, string prefix)
    {
        // Fetch latest remote branches if remote exists
        if (FetchService.HasRemote(repo))
        {
            try
            {
                FetchService.FetchAll(repo);
            }
            catch
            {
                // Continue with cached remote data if fetch fails
            }
        }

        var branches = new List<BranchInfo>();
        var currentBranch = repo.Head.FriendlyName;
        var processedBranches = new HashSet<string>();

        foreach (var branch in repo.Branches)
        {
            string branchNameToCheck;
            bool isRemoteRef = branch.IsRemote;

            if (isRemoteRef)
            {
                // Remote branch like "origin/feature/PIQ-123"
                if (branch.FriendlyName.StartsWith("origin/"))
                {
                    branchNameToCheck = branch.FriendlyName.Substring("origin/".Length);
                }
                else
                {
                    continue; // Skip non-origin remotes
                }
            }
            else
            {
                branchNameToCheck = branch.FriendlyName;
            }

            // Check if this branch matches the prefix
            if (!branchNameToCheck.StartsWith(prefix))
                continue;

            var shortName = branchNameToCheck.Substring(prefix.Length);

            // Skip if we already processed this branch name
            if (processedBranches.Contains(shortName))
                continue;

            processedBranches.Add(shortName);

            // Find both local and remote versions
            var localBranch = repo.Branches[branchNameToCheck];
            var remoteBranch = repo.Branches[$"origin/{branchNameToCheck}"];

            bool hasLocal = localBranch != null && !localBranch.IsRemote;
            bool hasRemote = remoteBranch != null;

            var displayBranch = hasLocal ? localBranch : remoteBranch;
            if (displayBranch == null) continue;

            var branchInfo = new BranchInfo
            {
                Name = shortName,
                FullName = branchNameToCheck,
                IsLocal = hasLocal,
                IsRemote = hasRemote,
                IsCurrentBranch = branchNameToCheck == currentBranch,
                Tip = displayBranch.Tip.Sha.Substring(0, 7),
                IsTracking = hasLocal && localBranch!.IsTracking,
                TrackedBranchName = hasLocal && localBranch!.IsTracking ? localBranch.TrackedBranch?.FriendlyName : null,
                AheadBy = 0,
                BehindBy = 0
            };

            // Calculate ahead/behind if tracking
            if (branchInfo.IsTracking && hasLocal && hasRemote)
            {
                try
                {
                    var trackingDetails = repo.Head.TrackingDetails;
                    if (localBranch!.FriendlyName == currentBranch && trackingDetails != null)
                    {
                        branchInfo.AheadBy = trackingDetails.AheadBy ?? 0;
                        branchInfo.BehindBy = trackingDetails.BehindBy ?? 0;
                    }
                }
                catch
                {
                    // Ignore tracking details errors
                }
            }

            branches.Add(branchInfo);
        }

        return branches.OrderBy(b => b.Name).ToList();
    }

    public static bool BranchExists(Repository repo, string branchName)
    {
        return repo.Branches[branchName] != null;
    }

    public static bool LocalAndRemoteInSync(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            return false;

        if (!branch.IsTracking)
            return true;

        return branch.Tip.Sha == branch.TrackedBranch.Tip.Sha;
    }

    public static bool BranchHasUnmergedCommits(Repository repo, string branchName, string targetBranchName)
    {
        var branch = repo.Branches[branchName];
        var targetBranch = repo.Branches[targetBranchName];

        if (branch == null || targetBranch == null)
            return false;

        var baseCommit = repo.ObjectDatabase.FindMergeBase(branch.Tip, targetBranch.Tip);
        return branch.Tip.Sha != baseCommit.Sha;
    }
}
