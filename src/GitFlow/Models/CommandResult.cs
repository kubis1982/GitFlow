namespace GitFlow.Models;

public class CommandResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public static CommandResult Ok(string? message = null) => new() { Success = true, Message = message };
    public static CommandResult Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}
