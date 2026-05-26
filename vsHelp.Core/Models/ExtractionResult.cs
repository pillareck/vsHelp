namespace vsHelp.Core.Models;

public class ExtractionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ExtractionPath { get; set; } = string.Empty;
    public List<string> SqlFilesFound { get; set; } = new();
    public List<string> Logs { get; set; } = new();

    public static ExtractionResult Succeeded(string path, List<string> sqlFiles) => new()
    {
        Success = true,
        ExtractionPath = path,
        SqlFilesFound = sqlFiles,
        Message = "Extração concluída com sucesso."
    };

    public static ExtractionResult Failed(string message) => new()
    {
        Success = false,
        Message = message
    };
}
