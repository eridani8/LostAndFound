using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Views;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace LostAndFound.ViewModels;

public partial class MainWindowViewModel(
    INavigationService navigationService,
    INavigationViewPageProvider navigationViewPageProvider,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository
) : ObservableObject
{
    public ObservableCollection<NavigationViewItem> MenuItems { get; } = [];

    [RelayCommand]
    private void WindowLoaded(FluentWindow window)
    {
        if (window is not MainWindow mainWindow)
            return;
        if (App.CurrentUser is not { } user)
            return;
        if (user.Role is not { } role)
            return;

        mainWindow.NavigationView.SetPageProviderService(navigationViewPageProvider);
        navigationService.SetNavigationControl(mainWindow.NavigationView);

        mainWindow.NavigationView.Navigated += NavigationViewOnNavigated;

        dialogService.SetDialogHost(mainWindow.ContentDialog);
        snackbarService.SetSnackbarPresenter(mainWindow.SnackbarPresenter);

        Type? homePage;

        switch (role.RoleId)
        {
            case 1:
                MenuItems.Add(
                    new NavigationViewItem(
                        "Пользователи",
                        SymbolRegular.People24,
                        typeof(UsersView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem(
                        "Категории",
                        SymbolRegular.AppFolder24,
                        typeof(CategoriesView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem(
                        "Потерянные предметы",
                        SymbolRegular.BoxSearch24,
                        typeof(LostItemsView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem(
                        "Возвращенные предметы",
                        SymbolRegular.Box24,
                        typeof(ReturnedItemsView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem(
                        "Места хранения",
                        SymbolRegular.FolderOpen24,
                        typeof(StorageLocationsView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem(
                        "Отчёты",
                        SymbolRegular.DocumentText24,
                        typeof(ReportsView)
                    )
                );
                MenuItems.Add(
                    new NavigationViewItem("Логи", SymbolRegular.CodeText20, typeof(LogsView))
                );
                homePage = typeof(UsersView);
                break;
            case 2:
                MenuItems.Add(
                    new NavigationViewItem(
                        "Потерянные предметы",
                        SymbolRegular.BoxSearch24,
                        typeof(LostItemsView)
                    )
                );
                homePage = typeof(LostItemsView);
                break;
            default:
                return;
        }

        navigationService.Navigate(homePage);
    }

    private void NavigationViewOnNavigated(NavigationView sender, NavigatedEventArgs args)
    {
        _ = logRepository.AddAsync(
            new ActionLog
            {
                ActionType = "OpenPage",
                Details = $"Открыл страницу {args.Page.GetType().Name}",
            }
        );
    }

    [RelayCommand]
    private void Close()
    {
        Application.Current.Shutdown();
    }
}
