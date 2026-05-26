using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace vsHelp.UI.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentViewName = "Restaurar Backup";

    [ObservableProperty]
    private string _title = "vsHelp";

    public MainViewModel()
    {
        NavigateToRestoreBackup();
    }

    [RelayCommand]
    private void NavigateToRestoreBackup()
    {
        CurrentView = new HomeViewModel();
        CurrentViewName = "Restaurar Backup";
    }

    [RelayCommand]
    private void NavigateToInstallations()
    {
        CurrentView = new InstallationsViewModel();
        CurrentViewName = "Instalações";
    }

    [RelayCommand]
    private void NavigateToHomologation()
    {
        CurrentView = new HomologationViewModel();
        CurrentViewName = "Homologação";
    }

    [RelayCommand]
    private void NavigateToVersions()
    {
        CurrentView = new VersionsViewModel();
        CurrentViewName = "Versões";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = new SettingsViewModel();
        CurrentViewName = "Configurações";
    }
}

public class HomeViewModel : ObservableObject { }
public class InstallationsViewModel : ObservableObject { }
public class HomologationViewModel : ObservableObject { }
public class VersionsViewModel : ObservableObject { }
public class SettingsViewModel : ObservableObject { }
