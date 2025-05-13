using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.ViewModels;

public partial class ReturnedItemsViewModel(ItemReturnRepository returnRepository) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemReturn> _returnedItems = [];

    [RelayCommand]
    private async Task LoadReturnedItemsAsync()
    {
        var items = await returnRepository.GetAllAsync();
        ReturnedItems = new ObservableCollection<ItemReturn>(items);
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
        await LoadReturnedItemsAsync();
    }
}
