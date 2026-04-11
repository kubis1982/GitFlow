using LibGit2Sharp;

namespace GitFlow.Services;

public class GitRepositoryService
{
    public bool IsGitRepository(string path = ".")
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

    public Repository GetRepository(string path = ".")
    {
        try
        {
            return new Repository(path);
        }
        catch (RepositoryNotFoundException)
        {
            throw new InvalidOperationException("Not a git repository. Run 'gitflow init' first.");
        }
    }

    public bool IsWorkingDirectoryClean(Repository repo)
    {
        var status = repo.RetrieveStatus();
        return status.IsDirty == false;
    }

    public string GetRepositoryRootPath(string path = ".")
    {
        var repo = GetRepository(path);
        return repo.Info.WorkingDirectory;
    }
}
