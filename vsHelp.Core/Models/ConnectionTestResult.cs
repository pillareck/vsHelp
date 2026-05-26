namespace vsHelp.Core.Models;

public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Databases { get; set; } = new();
}
