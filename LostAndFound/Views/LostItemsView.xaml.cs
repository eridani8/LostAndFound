using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class LostItemsView : UserControl
{
    public LostItemsView(LostItemsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
