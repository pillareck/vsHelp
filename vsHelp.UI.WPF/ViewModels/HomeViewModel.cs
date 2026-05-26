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

    [ObservableProperty]
    private string _statusMessage = "Aguardando seleção de arquivo...";

    [ObservableProperty]
    private BackupFile? _selectedFile;

    [ObservableProperty]
    private bool _isFileSelected;

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private bool _isBackupValid;

    [ObservableProperty]
    private double _operationProgress;

    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel()
    {
        _validationService = new BackupValidationService();
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

            // Call Validation Service
            OperationProgress = 20;
            var validationResult = await _validationService.ValidateBackupAsync(filePath);
            
            OperationProgress = 80;
            foreach (var log in validationResult.ValidationLogs)
            {
                AddLog(log);
            }

            IsBackupValid = validationResult.IsValid;
            StatusMessage = validationResult.IsValid ? "Backup validado com sucesso." : "Falha na validação do backup.";
            
            if (!IsBackupValid)
            {
                AddLog($"[WARNING] O backup selecionado não é válido para restauração automática.");
            }

            OperationProgress = 100;
        }
        catch (Exception ex)
        {
            AddLog($"[ERRO] Falha crítica ao processar arquivo: {ex.Message}");
            StatusMessage = "Erro fatal ao selecionar arquivo.";
            IsBackupValid = false;
        }
        finally
        {
            IsValidating = false;
        }
    }

    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add($"[{timestamp}] {message}");
    }
}
