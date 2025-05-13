using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class UsersView : UserControl
{
    public UsersView(UsersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}