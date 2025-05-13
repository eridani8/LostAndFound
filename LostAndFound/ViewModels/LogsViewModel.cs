using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.ViewModels;

public partial class LogsViewModel(ActionLogRepository logRepository) : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ActionLog> _logs = [];
    
    [RelayCommand]
    private async Task LoadLogsAsync()
    {
        var logs = await logRepository.GetAllAsync();
        Logs = new ObservableCollection<ActionLog>(logs);
    }
    
    [RelayCommand]
    private async Task WindowLoaded()
    {
        await LoadLogsAsync();
    }
}