namespace vsHelp.Core.Models;

public class RestoreResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Logs { get; set; } = new();

    public static RestoreResult Succeeded(string message) => new()
    {
        Success = true,
        Message = message
    };

    public static RestoreResult Failed(string message) => new()
    {
        Success = false,
        Message = message
    };
}
