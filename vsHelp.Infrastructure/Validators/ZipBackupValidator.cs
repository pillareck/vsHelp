using vsHelp.Core.Enums;
using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using System.IO;

namespace vsHelp.Infrastructure.Validators;

public class ZipBackupValidator : IBackupValidator
{
    public async Task<BackupValidationResult> ValidateAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return BackupValidationResult.Failure("Arquivo ZIP não encontrado.");

            // Check signature (PK..)
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            var buffer = new byte[4];
            await fs.ReadAsync(buffer, 0, 4);

            // ZIP signature: 50 4B 03 04
            if (buffer[0] == 0x50 && buffer[1] == 0x4B)
            {
                return BackupValidationResult.Success(BackupType.Zip, "Arquivo ZIP íntegro identificado.");
            }

            return BackupValidationResult.Failure("Assinatura de arquivo ZIP inválida.");
        }
        catch (Exception ex)
        {
            return BackupValidationResult.Failure($"Erro ao validar ZIP: {ex.Message}");
        }
    }
}
