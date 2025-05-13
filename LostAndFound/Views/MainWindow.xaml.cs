using LostAndFound.ViewModels;
using Wpf.Ui.Controls;

namespace LostAndFound.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}