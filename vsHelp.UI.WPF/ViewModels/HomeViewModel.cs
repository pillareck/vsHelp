using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using vsHelp.Core.Models;
using vsHelp.Application.Services;
using System.IO;

namespace vsHelp.UI.WPF.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly BackupValidationService _validationService;
    private readonly BackupExtractionService _extractionService;

    [ObservableProperty]
    private string _statusMessage = "Aguardando seleção de arquivo...";

    [ObservableProperty]
    private BackupFile? _selectedFile;

    [ObservableProperty]
    private bool _isFileSelected;

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private bool _isExtracting;

    [ObservableProperty]
    private bool _isBackupValid;

    [ObservableProperty]
    private double _operationProgress;

    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel()
    {
        _validationService = new BackupValidationService();
        _extractionService = new BackupExtractionService();
        AddLog("Sistema pronto para operação.");
    }

    [RelayCommand]
    private async Task SelectFileAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Arquivos de Backup (*.zip;*.rar;*.sql)|*.zip;*.rar;*.sql|Todos os arquivos (*.*)|*.*",
            Title = "Selecionar Arquivo de Backup"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await ProcessSelectedFile(openFileDialog.FileName);
        }
    }

    public async Task ProcessSelectedFile(string filePath)
    {
        try
        {
            IsValidating = true;
            IsExtracting = false;
            IsBackupValid = false;
            IsFileSelected = false;
            OperationProgress = 0;
            
            var fileInfo = new FileInfo(filePath);
            
            SelectedFile = new BackupFile
            {
                FullPath = fileInfo.FullName,
                FileName = fileInfo.Name,
                Extension = fileInfo.Extension.ToUpper().Replace(".", ""),
                SizeInBytes = fileInfo.Length,
                FormattedSize = BackupFile.GetFormattedSize(fileInfo.Length)
            };

            IsFileSelected = true;
            StatusMessage = "Validando arquivo: " + SelectedFile.FileName;
            AddLog($"[INFO] Arquivo selecionado: {SelectedFile.FileName}");

            // Step 1: Validation
            OperationProgress = 10;
            var validationResult = await _validationService.ValidateBackupAsync(filePath);
            
            foreach (var log in validationResult.ValidationLogs)
            {
                AddLog(log);
            }

            IsBackupValid = validationResult.IsValid;
            
            if (!IsBackupValid)
            {
                StatusMessage = "Falha na validação do backup.";
                AddLog($"[WARNING] O backup selecionado não é válido para restauração automática.");
                OperationProgress = 0;
                return;
            }

            StatusMessage = "Backup validado. Iniciando extração...";
            IsValidating = false;
            IsExtracting = true;

            // Step 2: Extraction (only if not .sql)
            var progressReporter = new Progress<double>(p => OperationProgress = 20 + (p * 0.8)); // Mapping 0-100 to 20-100 range
            
            var extractionResult = await _extractionService.PrepareBackupForRestoreAsync(filePath, progressReporter);
            
            foreach (var log in extractionResult.Logs)
            {
                AddLog(log);
            }

            if (extractionResult.Success)
            {
                if (extractionResult.SqlFilesFound.Any())
                {
                    AddLog($"[SUCCESS] Pronto para restauração. {extractionResult.SqlFilesFound.Count} arquivo(s) SQL localizados.");
                    StatusMessage = "Pronto para restaurar.";
                }
                else
                {
                    AddLog("[WARNING] Extração concluída, mas nenhum arquivo .sql foi encontrado no pacote.");
                    StatusMessage = "Aviso: SQL não encontrado.";
                }
            }
            else
            {
                AddLog($"[ERROR] Falha na extração: {extractionResult.Message}");
                StatusMessage = "Erro na extração.";
            }

            OperationProgress = 100;
        }
        catch (Exception ex)
        {
            AddLog($"[ERRO] Falha crítica no processamento: {ex.Message}");
            StatusMessage = "Erro fatal.";
            IsBackupValid = false;
        }
        finally
        {
            IsValidating = false;
            IsExtracting = false;
        }
    }

    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add($"[{timestamp}] {message}");
    }
}
