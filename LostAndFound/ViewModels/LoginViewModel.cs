using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Services;
using LostAndFound.Views;

namespace LostAndFound.ViewModels;

public partial class LoginViewModel(IUserService userService) : ObservableObject
{
    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoginAsync(object parameter)
    {
        if (string.IsNullOrWhiteSpace(Login))
        {
            ErrorMessage = "Введите логин";
            return;
        }

        if (
            parameter is not Wpf.Ui.Controls.PasswordBox passwordBox
            || string.IsNullOrWhiteSpace(passwordBox.Password)
        )
        {
            ErrorMessage = "Введите пароль";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await userService.AuthenticateAsync(Login, passwordBox.Password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            App.CurrentUser = user;

            var mainWindow = Application.Current.MainWindow;
            var newMainWindow = new MainWindow();
            Application.Current.MainWindow = newMainWindow;
            newMainWindow.Show();
            mainWindow?.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
