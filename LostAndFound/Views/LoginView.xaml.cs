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
}
