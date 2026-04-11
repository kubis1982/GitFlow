namespace GitFlow.Models;

public class BranchInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsLocal { get; set; }
    public bool IsRemote { get; set; }
    public bool IsCurrentBranch { get; set; }
    public string Tip { get; set; } = string.Empty;
}
