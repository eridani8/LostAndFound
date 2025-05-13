using System.Windows.Controls;
using LostAndFound.ViewModels;

namespace LostAndFound.Views;

public partial class CategoriesView : UserControl
{
    public CategoriesView(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
