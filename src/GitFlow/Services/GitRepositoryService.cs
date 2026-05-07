using LibGit2Sharp;

namespace GitFlow.Services;

public static class GitRepositoryService
{
    public static bool IsGitRepository(string path = ".")
    {
        try
        {
            _ = new Repository(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static Repository GetRepository(string path = ".")
    {
        try
        {
            return new Repository(path);
        }
        catch (RepositoryNotFoundException)
        {
            throw new InvalidOperationException("Not a git repository. Run 'gitflow config init' first.");
        }
    }

    public static bool IsWorkingDirectoryClean(Repository repo)
    {
        var status = repo.RetrieveStatus();
        return status.IsDirty == false;
    }

    public static string GetRepositoryRootPath(string path = ".")
    {
        var repo = GetRepository(path);
        return repo.Info.WorkingDirectory;
    }
}
