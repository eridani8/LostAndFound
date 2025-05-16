using System.Windows.Controls;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateLostItemDialog : UserControl
{
    private readonly CategoryRepository _categoryRepository;
    private readonly StorageLocationRepository _storageLocationRepository;
    private List<Category> Categories { get; set; } = [];
    private List<StorageLocation> Locations { get; set; } = [];

    public CreateLostItemDialog(
        CategoryRepository categoryRepository,
        StorageLocationRepository storageLocationRepository
    )
    {
        _categoryRepository = categoryRepository;
        _storageLocationRepository = storageLocationRepository;
        InitializeComponent();
        FoundDatePicker.SelectedDate = DateTime.Today;
    }

    public async Task LoadDataAsync()
    {
        Categories = (await _categoryRepository.GetAllAsync()).ToList();
        Locations = (await _storageLocationRepository.GetAllAsync()).ToList();

        CategoryComboBox.ItemsSource = Categories;
        StorageLocationComboBox.ItemsSource = Locations;

        if (Categories.Count > 0) CategoryComboBox.SelectedIndex = 0;
        if (Locations.Count > 0) StorageLocationComboBox.SelectedIndex = 0;
    }

    public LostItem? CreateLostItem(int userId)
    {
        if (string.IsNullOrEmpty(ItemNameInput.Text))
            return null;
        if (!FoundDatePicker.SelectedDate.HasValue)
            return null;
        if (string.IsNullOrEmpty(FoundLocationInput.Text))
            return null;
        if (CategoryComboBox.SelectedItem is not Category selectedCategory)
            return null;
        if (StorageLocationComboBox.SelectedItem is not StorageLocation selectedLocation)
            return null;

        return new LostItem
        {
            ItemName = ItemNameInput.Text,
            Description = DescriptionInput.Text,
            FoundDate = FoundDatePicker.SelectedDate.Value,
            FoundLocation = FoundLocationInput.Text,
            CategoryId = selectedCategory.CategoryId,
            StorageLocationId = selectedLocation.StorageLocationId,
            Status = "Waiting",
            ImagePath = string.IsNullOrWhiteSpace(ImagePathInput.Text) ? null : ImagePathInput.Text,
            RegisteredBy = userId,
            RegistrationDate = DateTime.Now,
        };
    }
}
