using vsHelp.Core.Enums;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using vsHelp.Infrastructure.Validators;
using System.IO;

namespace vsHelp.Application.Services;

public class BackupValidationService
{
    private readonly Dictionary<string, IBackupValidator> _validators;

    public BackupValidationService()
    {
        // Simple manual DI/Registry for now
        _validators = new Dictionary<string, IBackupValidator>(StringComparer.OrdinalIgnoreCase)
        {
            { ".zip", new ZipBackupValidator() },
            { ".rar", new RarBackupValidator() },
            { ".sql", new SqlBackupValidator() }
        };
    }

    public async Task<BackupValidationResult> ValidateBackupAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return BackupValidationResult.Failure("Caminho do arquivo inválido.");

        if (!File.Exists(filePath))
            return BackupValidationResult.Failure("Arquivo de backup não encontrado no diretório especificado.");

        var extension = Path.GetExtension(filePath).ToLower();
        
        if (!_validators.TryGetValue(extension, out var validator))
        {
            return BackupValidationResult.Failure($"Extensão '{extension}' não suportada pelo sistema.");
        }

        var result = await validator.ValidateAsync(filePath);
        
        // Add basic common info
        result.ValidationLogs.Add($"[INFO] Verificando arquivo: {Path.GetFileName(filePath)}");
        result.ValidationLogs.Add($"[INFO] Tamanho: {BackupFile.GetFormattedSize(new FileInfo(filePath).Length)}");
        
        if (result.IsValid)
        {
            result.ValidationLogs.Add($"[SUCCESS] {result.Message}");
        }
        else
        {
            result.ValidationLogs.Add($"[ERROR] {result.Message}");
        }

        return result;
    }
}
