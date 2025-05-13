using System.Windows;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
