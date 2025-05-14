using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateCategoryDialog : UserControl
{
    public CreateCategoryDialog()
    {
        InitializeComponent();
    }

    public Category? CreateCategory()
    {
        if (string.IsNullOrEmpty(CategoryName.Text)) return null;

        return new Category
        {
            CategoryName = CategoryName.Text,
            Description = Description.Text
        };
    }
}
