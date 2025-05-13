using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class LogsView : UserControl
{
    public LogsView(LogsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}