using vsHelp.Core.Enums;

namespace vsHelp.Core.Models;

public class BackupValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public BackupType DetectedType { get; set; } = BackupType.Unknown;
    public List<string> ValidationLogs { get; set; } = new();

    public static BackupValidationResult Success(BackupType type, string message) => new()
    {
        IsValid = true,
        DetectedType = type,
        Message = message
    };

    public static BackupValidationResult Failure(string message) => new()
    {
        IsValid = false,
        Message = message
    };
}
