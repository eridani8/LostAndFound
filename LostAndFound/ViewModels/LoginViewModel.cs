using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Services;
using LostAndFound.Views;

namespace LostAndFound.ViewModels;

public partial class LoginViewModel(
    IUserService userService,
    MainWindowViewModel mainWindowViewModel
) : ObservableObject, IDataErrorInfo
{
    [ObservableProperty] private string _login = string.Empty;

    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private bool _hasLoginError;

    [ObservableProperty] private bool _hasPasswordError;

    [ObservableProperty] private string _loginError = string.Empty;

    [ObservableProperty] private string _passwordError = string.Empty;

    public string Error => string.Empty;

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(Login):
                    if (string.IsNullOrWhiteSpace(Login))
                    {
                        HasLoginError = true;
                        return LoginError = "Введите логин";
                    }

                    if (Login.Length < 3)
                    {
                        HasLoginError = true;
                        return LoginError = "Логин должен содержать минимум 3 символа";
                    }

                    HasLoginError = false;
                    LoginError = string.Empty;
                    break;
            }

            return string.Empty;
        }
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            HasPasswordError = true;
            PasswordError = "Введите пароль";
            return false;
        }

        if (password.Length < 3)
        {
            HasPasswordError = true;
            PasswordError = "Пароль должен содержать минимум 3 символа";
            return false;
        }

        HasPasswordError = false;
        PasswordError = string.Empty;
        return true;
    }

    [RelayCommand]
    private async Task LoginAsync(object parameter)
    {
        if (parameter is not Wpf.Ui.Controls.PasswordBox passwordBox)
        {
            ErrorMessage = "Ошибка компонента пароля";
            return;
        }

        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Login))
        {
            HasLoginError = true;
            LoginError = "Введите логин";
            return;
        }

        if (!ValidatePassword(password))
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var user = await userService.AuthenticateAsync(Login, password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            var mainWindow = Application.Current.MainWindow;
            var newMainWindow = new MainWindow(mainWindowViewModel);
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