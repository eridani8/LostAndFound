using LostAndFound.ViewModels;
using Wpf.Ui.Controls;

namespace LostAndFound.Views;

public partial class LoginView : FluentWindow
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ValidatePassword(passwordBox.Password);
        }
    }
}
