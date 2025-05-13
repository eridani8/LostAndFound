using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.ViewModels;

public partial class UsersViewModel(UserRepository userRepository) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<User> _users = [];

    [ObservableProperty]
    private User? _selectedUser;

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        Users = new ObservableCollection<User>(users);
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
       await LoadUsersAsync();
    }
}
