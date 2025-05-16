using System.Windows.Controls;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateItemReturnDialog : UserControl
{
    private readonly LostItemRepository _lostItemRepository;

    private List<LostItem> LostItems { get; set; } = [];
    public CreateItemReturnDialog(LostItemRepository lostItemRepository)
    {
        _lostItemRepository = lostItemRepository;
        InitializeComponent();
        ReturnDatePicker.SelectedDate = DateTime.Today;
    }

    public async Task LoadDataAsync()
    {
        LostItems = (await _lostItemRepository.GetAllAsync())
            .Where(i => i.Status == "Waiting")
            .ToList();
        
        ItemComboBox.ItemsSource = LostItems;

        if (LostItems.Count > 0) ItemComboBox.SelectedIndex = 0;
    }

    public ItemReturn? CreateItemReturn(int userId)
    {
        if (string.IsNullOrEmpty(ReturnedToInput.Text)) return null;
        if (ItemComboBox.SelectedItem is not LostItem lostItem) return null;
        if (!ReturnDatePicker.SelectedDate.HasValue) return null;

        return new ItemReturn
        {
            ItemId = lostItem.ItemId,
            ReturnedTo = ReturnedToInput.Text,
            ReturnDate = ReturnDatePicker.SelectedDate.Value,
            ContactInfo = ContactInfoInput.Text,
            Notes = NotesInput.Text,
            ReceivedBy = userId,
        };
    }
}
