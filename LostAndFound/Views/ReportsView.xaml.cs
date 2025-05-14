using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class ReportsView : UserControl
{
    public ReportsView(ReportsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
