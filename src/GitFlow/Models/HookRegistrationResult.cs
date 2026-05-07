namespace GitFlow.Models;

/// <summary>
/// Result of hook registration operation
/// </summary>
public class HookRegistrationResult
{
    public int Copied { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    
    public bool Success => Failed == 0;
}
