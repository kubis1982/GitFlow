using LibGit2Sharp;
using GitFlow.Models;

namespace GitFlow.Services;

public class BranchService
{
    private readonly GitRepositoryService _gitService;

    public BranchService(GitRepositoryService gitService)
    {
        _gitService = gitService;
    }

    public void CreateBranch(Repository repo, string branchName, string sourceBranchName)
    {
        var sourceBranch = repo.Branches[sourceBranchName];
        if (sourceBranch == null)
            throw new InvalidOperationException($"Source branch '{sourceBranchName}' not found");

        var newBranch = repo.CreateBranch(branchName, sourceBranch.Tip);
        LibGit2Sharp.Commands.Checkout(repo, newBranch);
    }

    public void PublishBranch(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");

        var remote = repo.Network.Remotes["origin"];
        if (remote == null)
            throw new InvalidOperationException("No 'origin' remote found");

        repo.Network.Push(remote, $"refs/heads/{branchName}:refs/heads/{branchName}");
    }

    public void CheckoutBranch(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");

        LibGit2Sharp.Commands.Checkout(repo, branch);

        // Pull latest changes
        if (branch.IsTracking)
        {
            var options = new PullOptions
            {
                FetchOptions = new FetchOptions()
            };
            LibGit2Sharp.Commands.Pull(repo, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), options);
        }
    }

    public void DeleteBranch(Repository repo, string branchName, bool deleteRemote = true)
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

    public void UpdateBranch(Repository repo, string branchName, string parentBranchName)
    {
        var branch = repo.Branches[branchName];
        var parentBranch = repo.Branches[parentBranchName];

        if (branch == null)
            throw new InvalidOperationException($"Branch '{branchName}' not found");
        if (parentBranch == null)
            throw new InvalidOperationException($"Parent branch '{parentBranchName}' not found");

        LibGit2Sharp.Commands.Checkout(repo, branch);

        var mergeOptions = new MergeOptions();
        var result = repo.Merge(parentBranch, new Signature("GitFlow", "gitflow@local", DateTimeOffset.Now), mergeOptions);

        if (result.Status == MergeStatus.Conflicts)
        {
            throw new InvalidOperationException("Merge conflicts detected. Resolve them manually.");
        }
    }

    public List<BranchInfo> ListBranches(Repository repo, string prefix)
    {
        var branches = new List<BranchInfo>();

        foreach (var branch in repo.Branches)
        {
            if (branch.FriendlyName.StartsWith(prefix))
            {
                branches.Add(new BranchInfo
                {
                    Name = branch.FriendlyName.Substring(prefix.Length),
                    FullName = branch.FriendlyName,
                    IsLocal = branch.IsRemote == false,
                    IsRemote = branch.IsRemote,
                    Tip = branch.Tip.Sha.Substring(0, 7)
                });
            }
        }

        return branches;
    }

    public bool BranchExists(Repository repo, string branchName)
    {
        return repo.Branches[branchName] != null;
    }

    public bool LocalAndRemoteInSync(Repository repo, string branchName)
    {
        var branch = repo.Branches[branchName];
        if (branch == null)
            return false;

        if (!branch.IsTracking)
            return true;

        return branch.Tip.Sha == branch.TrackedBranch.Tip.Sha;
    }

    public bool BranchHasUnmergedCommits(Repository repo, string branchName, string targetBranchName)
    {
        var branch = repo.Branches[branchName];
        var targetBranch = repo.Branches[targetBranchName];

        if (branch == null || targetBranch == null)
            return false;

        var baseCommit = repo.ObjectDatabase.FindMergeBase(branch.Tip, targetBranch.Tip);
        return branch.Tip.Sha != baseCommit.Sha;
    }
}
