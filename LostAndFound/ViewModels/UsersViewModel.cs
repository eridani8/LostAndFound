using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LostAndFound.ViewModels;

public partial class UsersViewModel(UserRepository userRepository, ISnackbarService snackbarService, IContentDialogService dialogService)
    : ObservableObject
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

    [RelayCommand]
    private async Task DeleteUser(User user)
    {
        if (user.Login == "root")
        {
            snackbarService.Show("Ошибка", "Нельзя удалить root администратора", ControlAppearance.Danger);
            return;
        }
        
        var dialog = new ContentDialog
        {
            Title = "Подтверждение удаления",
            Content = $"Вы действительно хотите удалить пользователя {user.FullName}?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary) return;

        try
        {
            await userRepository.DeleteAsync(user.UserId);
            Users.Remove(user);
            snackbarService.Show("Успех", "Пользователь успешно удален", ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось удалить пользователя: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }
}
