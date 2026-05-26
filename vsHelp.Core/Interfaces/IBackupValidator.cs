using vsHelp.Core.Models;

namespace vsHelp.Core.Interfaces;

public interface IBackupValidator
{
    Task<BackupValidationResult> ValidateAsync(string filePath);
}
