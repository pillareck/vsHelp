using vsHelp.Core.Enums;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using System.IO;

namespace vsHelp.Infrastructure.Validators;

public class RarBackupValidator : IBackupValidator
{
    public async Task<BackupValidationResult> ValidateAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return BackupValidationResult.Failure("Arquivo RAR não encontrado.");

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var buffer = new byte[7];
            await fs.ReadAsync(buffer, 0, 7);

            // RAR 4.x signature: 52 61 72 21 1A 07 00
            // RAR 5.0 signature: 52 61 72 21 1A 07 01 00
            if (buffer[0] == 0x52 && buffer[1] == 0x61 && buffer[2] == 0x72 && buffer[3] == 0x21)
            {
                return BackupValidationResult.Success(BackupType.Rar, "Arquivo RAR identificado.");
            }

            return BackupValidationResult.Failure("Assinatura de arquivo RAR inválida.");
        }
        catch (Exception ex)
        {
            return BackupValidationResult.Failure($"Erro ao validar RAR: {ex.Message}");
        }
    }
}
