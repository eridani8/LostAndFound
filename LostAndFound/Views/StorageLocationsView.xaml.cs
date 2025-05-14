using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class StorageLocationsView : UserControl
{
    public StorageLocationsView(StorageLocationsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
