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
    INavigationViewPageProvider navigationViewPageProvider,
    ISnackbarService snackbarService,
    IContentDialogService dialogService) : ObservableObject
{
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void WindowLoaded(FluentWindow window)
    {
        if (window is not MainWindow mainWindow) return;
        if (App.CurrentUser is not { } user) return;
        if (user.Role is not { } role) return;
        
        mainWindow.NavigationView.SetPageProviderService(navigationViewPageProvider);
        navigationService.SetNavigationControl(mainWindow.NavigationView);
        
        dialogService.SetDialogHost(mainWindow.ContentDialog);
        snackbarService.SetSnackbarPresenter(mainWindow.SnackbarPresenter);

        Type? homePage;
        
        switch (role.RoleId)
        {
            case 1:
                MenuItems.Add(new NavigationViewItem("Пользователи", SymbolRegular.People24, typeof(UsersView)));
                MenuItems.Add(new NavigationViewItem("Логи", SymbolRegular.CodeText20, typeof(LogsView)));
                homePage = typeof(UsersView);
                break;
            default:
                return;
        } // TODO

        navigationService.Navigate(homePage);
    }
}