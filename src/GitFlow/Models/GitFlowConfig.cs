namespace GitFlow.Models;

public class GitFlowConfig
{
    public string ProductionBranch { get; set; } = "main";
    public string DevelopmentBranch { get; set; } = "develop";
    public string FeaturePrefix { get; set; } = "feature/";
    public string ReleasePrefix { get; set; } = "release/";
    public string HotfixPrefix { get; set; } = "hotfix/";
    public string BugfixPrefix { get; set; } = "bugfix/";
    public string VersionPrefix { get; set; } = "v";
    public string MergeStrategy { get; set; } = "--no-ff";
    public bool IsGlobal { get; set; }

    public bool IsInitialized => !string.IsNullOrEmpty(ProductionBranch);
}
