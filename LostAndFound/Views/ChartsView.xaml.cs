using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class ChartsView : UserControl
{
    public ChartsView(ChartsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
