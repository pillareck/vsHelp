using vsHelp.Core.Models;
using vsHelp.Application.Services;
using vsHelp.Core.Interfaces;
using vsHelp.Infrastructure.Services;
using System.IO;

namespace vsHelp.Application.Orchestrators;

public class BackupRestoreOrchestrator
{
    private readonly BackupValidationService _validationService;
    private readonly BackupExtractionService _extractionService;
    private readonly IDatabaseRestoreService _restoreService;

    public BackupRestoreOrchestrator()
    {
        _validationService = new BackupValidationService();
        _extractionService = new BackupExtractionService();
        _restoreService = new MySqlRestoreService();
    }

    public async Task RunRestoreAsync(string filePath, DatabaseConnectionSettings dbSettings, IProgress<double> progress, Action<string> logger)
    {
        try
        {
            // 1. Validation
            logger($"[INFO] Iniciando validação: {Path.GetFileName(filePath)}");
            progress.Report(5);
            var validationResult = await _validationService.ValidateBackupAsync(filePath);
            foreach (var log in validationResult.ValidationLogs) logger(log);

            if (!validationResult.IsValid)
            {
                logger("[ERROR] Falha na validação do backup.");
                return;
            }

            // 2. Extraction
            logger("[INFO] Iniciando extração do pacote...");
            progress.Report(15);
            var extractionProgress = new Progress<double>(p => progress.Report(15 + (p * 0.35))); // 15% to 50%
            var extractionResult = await _extractionService.PrepareBackupForRestoreAsync(filePath, extractionProgress);
            foreach (var log in extractionResult.Logs) logger(log);

            if (!extractionResult.Success || !extractionResult.SqlFilesFound.Any())
            {
                logger("[ERROR] Falha na extração ou nenhum arquivo SQL localizado.");
                return;
            }

            // 3. MySQL Restore
            var sqlFile = extractionResult.SqlFilesFound.First(); // For now, restore first one
            logger($"[INFO] Iniciando restauração do banco: {dbSettings.DatabaseName}");
            progress.Report(55);
            
            var restoreProgress = new Progress<double>(p => progress.Report(55 + (p * 0.45))); // 55% to 100%
            var restoreResult = await _restoreService.RestoreAsync(sqlFile, dbSettings, restoreProgress);
            
            foreach (var log in restoreResult.Logs) logger(log);

            if (restoreResult.Success)
            {
                logger("[SUCCESS] Fluxo de restauração concluído com sucesso!");
                progress.Report(100);
            }
            else
            {
                logger($"[ERROR] Falha na restauração: {restoreResult.Message}");
                progress.Report(0);
            }
        }
        catch (Exception ex)
        {
            logger($"[ERROR] Erro crítico no orquestrador: {ex.Message}");
            progress.Report(0);
        }
        finally
        {
            // Optional: Clean temp files
            // _extractionService.CleanTempFiles();
        }
    }
}
