using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using vsHelp.Core.Models;
using vsHelp.Application.Orchestrators;
using System.IO;

namespace vsHelp.UI.WPF.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly BackupRestoreOrchestrator _orchestrator;

    [ObservableProperty]
    private string _statusMessage = "Aguardando seleção de arquivo...";

    [ObservableProperty]
    private BackupFile? _selectedFile;

    [ObservableProperty]
    private bool _isFileSelected;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private bool _isBackupValid;

    [ObservableProperty]
    private double _operationProgress;

    // MySQL Fake/Manual Settings for now
    [ObservableProperty]
    private string _dbServer = "localhost";
    
    [ObservableProperty]
    private string _dbName = "vs_help_restore";

    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel()
    {
        _orchestrator = new BackupRestoreOrchestrator();
        AddLog("Sistema pronto para operação.");
    }

    [RelayCommand]
    private async Task SelectAndRestoreAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Arquivos de Backup (*.zip;*.rar;*.sql)|*.zip;*.rar;*.sql|Todos os arquivos (*.*)|*.*",
            Title = "Selecionar Backup para Restauração"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await RunFullRestoreFlow(openFileDialog.FileName);
        }
    }

    private async Task RunFullRestoreFlow(string filePath)
    {
        try
        {
            IsProcessing = true;
            IsBackupValid = true; // Visual state
            Logs.Clear();
            
            var dbSettings = new DatabaseConnectionSettings
            {
                Server = DbServer,
                DatabaseName = DbName,
                User = "root",
                Password = "" // Should be handled securely in real scenario
            };

            var progress = new Progress<double>(p => OperationProgress = p);
            
            await _orchestrator.RunRestoreAsync(filePath, dbSettings, progress, AddLog);
            
            StatusMessage = OperationProgress >= 100 ? "Restauração Finalizada!" : "Falha na operação.";
        }
        catch (Exception ex)
        {
            AddLog($"[ERROR] Falha na UI: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void AddLog(string message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Logs.Add($"[{timestamp}] {message}");
        });
    }
}
