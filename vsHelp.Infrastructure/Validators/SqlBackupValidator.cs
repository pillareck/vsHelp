using vsHelp.Core.Enums;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using System.IO;
using System.Text;

namespace vsHelp.Infrastructure.Validators;

public class SqlBackupValidator : IBackupValidator
{
    public async Task<BackupValidationResult> ValidateAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return BackupValidationResult.Failure("Arquivo SQL não encontrado.");

            // Check if it's a text file with SQL markers
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var buffer = new byte[1024];
            int read = await fs.ReadAsync(buffer, 0, 1024);
            var content = Encoding.UTF8.GetString(buffer, 0, read).ToUpper();

            // Common MySQL dump markers
            if (content.Contains("CREATE TABLE") || 
                content.Contains("INSERT INTO") || 
                content.Contains("MYSQL DUMP") ||
                content.Contains("--") ||
                content.Contains("SET "))
            {
                return BackupValidationResult.Success(BackupType.Sql, "Script SQL identificado.");
            }

            return BackupValidationResult.Failure("O arquivo não parece ser um script SQL válido.");
        }
        catch (Exception ex)
        {
            return BackupValidationResult.Failure($"Erro ao validar SQL: {ex.Message}");
        }
    }
}
