using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateItemReturnDialog : UserControl
{
    public CreateItemReturnDialog()
    {
        InitializeComponent();
        ReturnDatePicker.SelectedDate = DateTime.Today;
    }

    public ItemReturn? CreateItemReturn(int userId)
    {
        if (string.IsNullOrEmpty(ReturnedToInput.Text)) return null;
        if (!int.TryParse(ItemIdInput.Text, out var itemId)) return null;
        if (!ReturnDatePicker.SelectedDate.HasValue) return null;

        return new ItemReturn
        {
            ItemId = itemId,
            ReturnedTo = ReturnedToInput.Text,
            ReturnDate = ReturnDatePicker.SelectedDate.Value,
            ContactInfo = ContactInfoInput.Text,
            Notes = NotesInput.Text,
            ReceivedBy = userId,
        };
    }
}
