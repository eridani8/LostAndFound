using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class ReturnedItemsView : UserControl
{
    public ReturnedItemsView(ReturnedItemsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
