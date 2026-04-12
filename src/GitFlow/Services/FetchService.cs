using LibGit2Sharp;
using GitFlow.Utilities;

namespace GitFlow.Services;

public static class FetchService
{
    public static void Fetch(Repository repo, string remoteName = "origin", string? branchName = null)
    {
        var remote = repo.Network.Remotes[remoteName];
        if (remote == null)
            throw new InvalidOperationException($"Remote '{remoteName}' not found. Configure a remote repository first.");

        try
        {
            var refSpecs = branchName != null
                ? new[] { $"+refs/heads/{branchName}:refs/remotes/{remoteName}/{branchName}" }
                : remote.FetchRefSpecs.Select(x => x.Specification);

            var options = new FetchOptions
            {
                OnProgress = (serverProgressOutput) =>
                {
                    return true;
                },
                OnTransferProgress = (progress) =>
                {
                    return true;
                }
            };

            LibGit2Sharp.Commands.Fetch(repo, remoteName, refSpecs, options, null);
        }
        catch (LibGit2SharpException ex) when (ex.Message.Contains("failed to resolve address") || 
                                                ex.Message.Contains("failed to connect") ||
                                                ex.Message.Contains("Could not resolve host"))
        {
            throw new InvalidOperationException($"Network error: Unable to connect to remote '{remoteName}'. Check your internet connection.", ex);
        }
        catch (LibGit2SharpException ex)
        {
            throw new InvalidOperationException($"Failed to fetch from '{remoteName}': {ex.Message}", ex);
        }
    }

    public static void FetchAll(Repository repo, string remoteName = "origin")
    {
        Fetch(repo, remoteName, null);
    }

    public static bool RemoteBranchExists(Repository repo, string branchName, string remoteName = "origin")
    {
        try
        {
            Fetch(repo, remoteName);
            var remoteBranchName = $"{remoteName}/{branchName}";
            return repo.Branches[remoteBranchName] != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool HasRemote(Repository repo, string remoteName = "origin")
    {
        return repo.Network.Remotes[remoteName] != null;
    }
}
