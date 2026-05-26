using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using vsHelp.Core.Models;
using vsHelp.Application.Orchestrators;
using vsHelp.Application.Services;
using System.IO;

namespace vsHelp.UI.WPF.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly BackupRestoreOrchestrator _orchestrator;
    private readonly ConnectionManagerService _connectionManager;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _statusMessage = "Aguardando configuração de conexão...";

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _operationProgress;

    // File Selection Properties
    [ObservableProperty]
    private BackupFile? _selectedFile;

    [ObservableProperty]
    private bool _isFileSelected;

    // Connection Properties
    [ObservableProperty]
    private string _dbServer = "localhost";
    
    [ObservableProperty]
    private int _dbPort = 3306;

    [ObservableProperty]
    private string _dbUser = "root";

    [ObservableProperty]
    private string _dbPassword = "";

    [ObservableProperty]
    private string _dbName = "";

    [ObservableProperty]
    private bool _isTestingConnection;

    [ObservableProperty]
    private bool _isConnectionValid;

    [ObservableProperty]
    private ObservableCollection<string> _databases = new();

    [ObservableProperty]
    private string _selectedDatabase = "";

    [ObservableProperty]
    private bool _isCreatingNewDatabase;

    [ObservableProperty]
    private string _newDatabaseName = "";

    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _orchestrator = new BackupRestoreOrchestrator();
        _connectionManager = new ConnectionManagerService();
        AddLog("Painel de restauração carregado.");
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        try
        {
            IsTestingConnection = true;
            IsConnectionValid = false;
            Databases.Clear();
            AddLog($"[INFO] Testando conexão com {DbServer}:{DbPort}...");

            var settings = GetCurrentSettings();
            var result = await _connectionManager.ConnectAndListDatabasesAsync(settings);

            if (result.Success)
            {
                IsConnectionValid = true;
                foreach (var db in result.Databases) Databases.Add(db);
                Databases.Add("+ Criar novo banco...");
                
                AddLog("[SUCCESS] Conexão MySQL estabelecida.");
                AddLog($"[INFO] {result.Databases.Count} bancos de dados localizados.");
                StatusMessage = "Conexão válida. Selecione o banco de dados.";
                _mainViewModel.UpdateConnectionStatus(DbServer, "");
            }
            else
            {
                AddLog($"[ERROR] {result.Message}");
                StatusMessage = "Falha na conexão MySQL.";
            }
        }
        catch (Exception ex)
        {
            AddLog($"[ERROR] Erro ao testar conexão: {ex.Message}");
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    partial void OnSelectedDatabaseChanged(string value)
    {
        if (value == "+ Criar novo banco...")
        {
            IsCreatingNewDatabase = true;
            return;
        }

        IsCreatingNewDatabase = false;
        DbName = value;
        if (!string.IsNullOrEmpty(value))
        {
            _mainViewModel.UpdateConnectionStatus(DbServer, value);
            AddLog($"[INFO] Banco selecionado: {value}");
        }
    }

    [RelayCommand]
    private async Task CreateDatabaseAsync()
    {
        if (string.IsNullOrWhiteSpace(NewDatabaseName)) return;

        try
        {
            AddLog($"[INFO] Criando banco de dados: {NewDatabaseName}...");
            var success = await _connectionManager.CreateDatabaseAsync(GetCurrentSettings(), NewDatabaseName);
            
            if (success)
            {
                AddLog($"[SUCCESS] Banco '{NewDatabaseName}' criado com sucesso.");
                Databases.Insert(Databases.Count - 1, NewDatabaseName);
                SelectedDatabase = NewDatabaseName;
                IsCreatingNewDatabase = false;
            }
            else
            {
                AddLog("[ERROR] Falha ao criar banco de dados.");
            }
        }
        catch (Exception ex)
        {
            AddLog($"[ERROR] Erro na criação: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SelectAndRestoreAsync()
    {
        if (string.IsNullOrEmpty(DbName))
        {
            AddLog("[WARNING] Selecione um banco de dados antes de iniciar.");
            return;
        }

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

            AddLog("--------------------------------------------------");
            AddLog($"[INFO] Arquivo selecionado: {SelectedFile.FileName}");
            AddLog($"[INFO] Iniciando pipeline de restauração...");
            
            var dbSettings = GetCurrentSettings();
            dbSettings.DatabaseName = DbName;

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

    private DatabaseConnectionSettings GetCurrentSettings() => new()
    {
        Server = DbServer,
        Port = DbPort,
        User = DbUser,
        Password = DbPassword
    };

    private void AddLog(string message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Logs.Add($"[{timestamp}] {message}");
        });
    }
}
