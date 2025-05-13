using LostAndFound.ViewModels;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace LostAndFound.Views;

public partial class MainWindow : FluentWindow
{
    private readonly IContentDialogService _contentDialogService;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
