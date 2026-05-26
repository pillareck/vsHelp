using vsHelp.Core.Interfaces;
using vsHelp.Core.Models;
using vsHelp.Infrastructure.Extractors;
using System.IO;

namespace vsHelp.Application.Services;

public class BackupExtractionService
{
    private readonly IArchiveExtractor _extractor;
    private readonly string _baseTempPath;

    public BackupExtractionService()
    {
        _extractor = new SharpCompressArchiveExtractor();
        _baseTempPath = Path.Combine(Path.GetTempPath(), "vsHelp", "Extractions");
    }

    public async Task<ExtractionResult> PrepareBackupForRestoreAsync(string archivePath, IProgress<double>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(archivePath))
            return ExtractionResult.Failed("Caminho do backup inválido.");

        if (!File.Exists(archivePath))
            return ExtractionResult.Failed("Arquivo de backup não encontrado.");

        var extension = Path.GetExtension(archivePath).ToLower();
        
        // If it's already a SQL file, no extraction needed
        if (extension == ".sql")
        {
            return ExtractionResult.Succeeded(Path.GetDirectoryName(archivePath) ?? "", new List<string> { archivePath });
        }

        // Create a unique temp folder for this extraction
        var operationId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var destinationPath = Path.Combine(_baseTempPath, operationId);

        try
        {
            if (Directory.Exists(destinationPath))
                Directory.Delete(destinationPath, true);

            Directory.CreateDirectory(destinationPath);
            
            return await _extractor.ExtractAsync(archivePath, destinationPath, progress);
        }
        catch (Exception ex)
        {
            return ExtractionResult.Failed($"Erro ao preparar diretório temporário: {ex.Message}");
        }
    }

    public void CleanTempFiles()
    {
        try
        {
            if (Directory.Exists(_baseTempPath))
                Directory.Delete(_baseTempPath, true);
        }
        catch
        {
            // Best effort
        }
    }
}
