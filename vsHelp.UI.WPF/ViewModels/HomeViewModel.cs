using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using vsHelp.Core.Models;
using System.IO;

namespace vsHelp.UI.WPF.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Aguardando seleção de arquivo...";

    [ObservableProperty]
    private BackupFile? _selectedFile;

    [ObservableProperty]
    private bool _isFileSelected;

    [ObservableProperty]
    private double _operationProgress;

    public ObservableCollection<string> Logs { get; } = new();

    public HomeViewModel()
    {
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
            StatusMessage = "Arquivo selecionado: " + SelectedFile.FileName;
            
            AddLog($"[INFO] Backup selecionado: {SelectedFile.FileName} ({SelectedFile.FormattedSize})");
            AddLog($"[INFO] Caminho: {SelectedFile.FullPath}");
            
            // Simular um estado visual de carregamento ou validação inicial rápida
            OperationProgress = 0;
            for (int i = 0; i <= 100; i += 10)
            {
                OperationProgress = i;
                await Task.Delay(20);
            }
        }
        catch (Exception ex)
        {
            AddLog($"[ERRO] Falha ao processar arquivo: {ex.Message}");
            StatusMessage = "Erro ao selecionar arquivo.";
        }
    }

    private void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add($"[{timestamp}] {message}");
    }
}
