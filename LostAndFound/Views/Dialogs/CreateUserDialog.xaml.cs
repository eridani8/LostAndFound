using System.Windows.Controls;
using LostAndFound.Models;

namespace LostAndFound.Views.Dialogs;

public partial class CreateUserDialog : UserControl
{
    public CreateUserDialog()
    {
        InitializeComponent();
    }

    public User? CreateUser()
    {
        if (string.IsNullOrEmpty(Login.Text)) return null;
        if (string.IsNullOrEmpty(Password.Password)) return null;
        if (string.IsNullOrEmpty(FullName.Text)) return null;
        if (string.IsNullOrEmpty(Email.Text)) return null;
        if (string.IsNullOrEmpty(Phone.Text)) return null;

        return new User
        {
            Login = Login.Text,
            PasswordHash = Password.Password,
            FullName = FullName.Text,
            Email = Email.Text,
            Phone = Phone.Text,
            RoleId = 2,
            CreatedDate = DateTime.Now,
            IsActive = true
        };
    }
}
