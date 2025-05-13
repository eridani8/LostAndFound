using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Views;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace LostAndFound.ViewModels;

public partial class MainWindowViewModel(
    INavigationService navigationService,
    INavigationViewPageProvider navigationViewPageProvider) : ObservableObject
{
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void WindowLoaded(NavigationView navigationView)
    {
        if (App.CurrentUser is not { } user) return;
        if (user.Role is not { } role) return;
        
        navigationView.SetPageProviderService(navigationViewPageProvider);
        navigationService.SetNavigationControl(navigationView);

        Type? homePage;
        
        switch (role.RoleId)
        {
            case 1:
                MenuItems.Add(new NavigationViewItem("Пользователи", SymbolRegular.People24, typeof(UsersView)));
                homePage = typeof(UsersView);
                break;
            default:
                return;
        } // TODO

        navigationService.Navigate(homePage);
    }
}