using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateLostItemDialog : UserControl
{
    public CreateLostItemDialog()
    {
        InitializeComponent();
        FoundDatePicker.SelectedDate = DateTime.Today;
    }

    public LostItem? CreateLostItem(int userId)
    {
        if (string.IsNullOrEmpty(ItemNameInput.Text))
            return null;
        if (!FoundDatePicker.SelectedDate.HasValue)
            return null;
        if (string.IsNullOrEmpty(FoundLocationInput.Text))
            return null;
        if (!int.TryParse(CategoryIdInput.Text, out var categoryId))
            return null;
        if (!int.TryParse(StorageLocationIdInput.Text, out var storageLocationId))
            return null;

        return new LostItem
        {
            ItemName = ItemNameInput.Text,
            Description = DescriptionInput.Text,
            FoundDate = FoundDatePicker.SelectedDate.Value,
            FoundLocation = FoundLocationInput.Text,
            CategoryId = categoryId,
            StorageLocationId = storageLocationId,
            Status = "Waiting",
            ImagePath = string.IsNullOrWhiteSpace(ImagePathInput.Text) ? null : ImagePathInput.Text,
            RegisteredBy = userId,
            RegistrationDate = DateTime.Now,
        };
    }
}
