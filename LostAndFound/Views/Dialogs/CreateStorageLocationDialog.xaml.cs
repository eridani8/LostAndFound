using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateStorageLocationDialog : UserControl
{
    public CreateStorageLocationDialog()
    {
        InitializeComponent();
    }

    public StorageLocation? CreateStorageLocation()
    {
        if (string.IsNullOrWhiteSpace(LocationNameInput.Text))
            return null;
        if (string.IsNullOrWhiteSpace(AddressInput.Text))
            return null;
        return new StorageLocation
        {
            LocationName = LocationNameInput.Text,
            Address = AddressInput.Text,
            ContactPhone = string.IsNullOrWhiteSpace(ContactPhoneInput.Text)
                ? null
                : ContactPhoneInput.Text,
            WorkingHours = string.IsNullOrWhiteSpace(WorkingHoursInput.Text)
                ? null
                : WorkingHoursInput.Text,
        };
    }
}
