using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LostAndFound.ViewModels;

public partial class UsersViewModel(
    UserRepository userRepository,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<User> _users = [];

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var users = await userRepository.GetAllAsync();
        Users = new ObservableCollection<User>(users);
    }

    [RelayCommand]
    private async Task UpdateUser(DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is User user)
            {
                await UpdateUserAsync(user);
            }
        }
    }

    [RelayCommand]
    private async Task CreateUser()
    {
        var createUserControl = new CreateUserDialog();

        var dialog = new ContentDialog
        {
            Title = "Создание пользователя",
            Content = createUserControl,
            CloseButtonText = "Отмена",
            PrimaryButtonText = "Создать",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        var newUser = createUserControl.CreateUser();
        if (newUser == null)
        {
            snackbarService.Show(
                "Ошибка",
                "Заполните все обязательные поля",
                ControlAppearance.Danger
            );
            return;
        }

        try
        {
            newUser.PasswordHash = newUser.PasswordHash.HashPassword();

            var userId = await userRepository.AddAsync(newUser);
            var createdUser = await userRepository.GetByIdAsync(userId) ?? newUser;
            Users.Insert(0, createdUser);

            snackbarService.Show("Успех", "Пользователь успешно создан", ControlAppearance.Success);

            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "CreateUser",
                    Details = $"Создан новый пользователь [{newUser.Login}]",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось создать пользователя: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            var currentUser = await userRepository.GetByIdAsync(user.UserId);
            if (currentUser == null)
            {
                snackbarService.Show("Ошибка", "Пользователь не найден", ControlAppearance.Danger);
                return;
            }

            user.PasswordHash = currentUser.PasswordHash;
            user.CreatedDate = currentUser.CreatedDate;

            var result = await userRepository.UpdateAsync(user);

            if (result)
            {
                snackbarService.Show(
                    "Успех",
                    "Данные пользователя обновлены",
                    ControlAppearance.Success
                );

                if (Users.FirstOrDefault(u => u.UserId == user.UserId) is { } updatedUser)
                {
                    var index = Users.IndexOf(updatedUser);
                    if (index >= 0)
                    {
                        Users[index] = updatedUser;
                    }
                }

                await logRepository.AddAsync(
                    new ActionLog
                    {
                        ActionType = "UpdateUser",
                        Details = $"Обновлен пользователь: {user.FullName}",
                    }
                );
            }
            else
            {
                snackbarService.Show(
                    "Ошибка",
                    "Не удалось обновить пользователя",
                    ControlAppearance.Danger
                );
            }
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось обновить пользователя: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task DeleteUser(User user)
    {
        if (user.Login == "root")
        {
            snackbarService.Show(
                "Ошибка",
                "Нельзя удалить root администратора",
                ControlAppearance.Danger
            );
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

        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            await userRepository.DeleteAsync(user.UserId);
            Users.Remove(user);
            snackbarService.Show("Успех", "Пользователь успешно удален", ControlAppearance.Success);
            await logRepository.AddAsync(
                new ActionLog { ActionType = "RemoveUser", Details = "Пользователь удален" }
            );
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
