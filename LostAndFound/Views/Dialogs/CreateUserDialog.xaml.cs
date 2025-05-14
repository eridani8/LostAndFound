using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateUserDialog : UserControl
{
    public User? CreatedUser { get; private set; }

    public CreateUserDialog()
    {
        InitializeComponent();
    }

    public User? GetUserData()
    {
        if (
            string.IsNullOrWhiteSpace(LoginTextBox.Text)
            || string.IsNullOrWhiteSpace(PasswordBox.Password)
            || string.IsNullOrWhiteSpace(FullNameTextBox.Text)
        )
        {
            return null;
        }

        return new User
        {
            Login = LoginTextBox.Text,
            PasswordHash = "",
            FullName = FullNameTextBox.Text,
            Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text,
            Phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text,
            RoleId = 2,
            IsActive = true,
            CreatedDate = DateTime.Now,
        };
    }

    public string GetPassword()
    {
        return PasswordBox.Password;
    }
}
