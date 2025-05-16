using System.Collections.ObjectModel;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LostAndFound.ViewModels;

public partial class LostItemsViewModel(
    LostItemRepository lostItemRepository,
    CategoryRepository categoryRepository,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository,
    StorageLocationRepository storageLocationRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<LostItem> _lostItems = [];

    [ObservableProperty]
    private string _itemsCountText = "Найдено предметов: 0";

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _statuses = ["Waiting", "Found", "Returned"];

    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private ObservableCollection<string> _locations = [];

    [ObservableProperty]
    private string? _selectedLocation;

    public int CurrentUserRoleId => App.CurrentUser?.RoleId ?? 0;

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var items = (await lostItemRepository.GetAllAsync()).ToList();
        LostItems = new ObservableCollection<LostItem>(items);
        ItemsCountText = $"Найдено предметов: {LostItems.Count}";

        var allCategories = await categoryRepository.GetAllAsync();

        var categoriesList = new List<Category>
        {
            new() { CategoryId = -1, CategoryName = "Все категории" },
        };
        categoriesList.AddRange(allCategories);
        Categories = new ObservableCollection<Category>(categoriesList);

        Statuses = new ObservableCollection<string>(
            ["Все статусы", "Waiting", "Found", "Returned"]
        );

        var uniqueLocations = items
            .Select(i => i.FoundLocation)
            .Where(loc => !string.IsNullOrEmpty(loc))
            .Distinct()
            .OrderBy(loc => loc)
            .ToList();

        var locationsList = new List<string> { "Все места" };
        locationsList.AddRange(uniqueLocations);
        Locations = new ObservableCollection<string>(locationsList);
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        try
        {
            var filteredItems = await lostItemRepository.GetWithFiltersAsync(
                searchTerm: string.IsNullOrEmpty(SearchTerm) ? null : SearchTerm,
                categoryId: (SelectedCategory != null && SelectedCategory.CategoryId != -1)
                    ? SelectedCategory.CategoryId
                    : null,
                status: (SelectedStatus != null && SelectedStatus != "Все статусы")
                    ? SelectedStatus
                    : null,
                fromDate: FromDate,
                toDate: ToDate,
                location: (SelectedLocation != null && SelectedLocation != "Все места")
                    ? SelectedLocation
                    : null
            );

            LostItems = new ObservableCollection<LostItem>(filteredItems);
            ItemsCountText = $"Найдено предметов: {LostItems.Count}";

            var filtersApplied =
                !string.IsNullOrEmpty(SearchTerm)
                || (SelectedCategory != null && SelectedCategory.CategoryId != -1)
                || (SelectedStatus != null && SelectedStatus != "Все статусы")
                || FromDate.HasValue
                || ToDate.HasValue
                || (SelectedLocation != null && SelectedLocation != "Все места");

            var message = filtersApplied ? "Фильтры успешно применены" : "Показаны все записи";

            snackbarService.Show("Фильтрация", message, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось применить фильтры: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task ResetFilters()
    {
        try
        {
            SearchTerm = string.Empty;
            SelectedCategory = null;
            SelectedStatus = null;
            FromDate = null;
            ToDate = null;
            SelectedLocation = null;

            var items = await lostItemRepository.GetAllAsync();
            LostItems = new ObservableCollection<LostItem>(items);
            ItemsCountText = $"Найдено предметов: {LostItems.Count}";

            snackbarService.Show("Фильтрация", "Фильтры сброшены", ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось сбросить фильтры: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task CreateLostItem()
    {
        if (App.CurrentUser is not { } currentUser)
            return;

        if (currentUser.RoleId == 2)
        {
            snackbarService.Show(
                "Доступ запрещен",
                "У вас нет прав на создание предметов",
                ControlAppearance.Caution
            );
            return;
        }

        var createLostItemControl = new CreateLostItemDialog(categoryRepository, storageLocationRepository);
        await createLostItemControl.LoadDataAsync();

        var dialog = new ContentDialog
        {
            Title = "Добавить потерянный предмет",
            Content = createLostItemControl,
            CloseButtonText = "Отмена",
            PrimaryButtonText = "Создать",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        var newLostItem = createLostItemControl.CreateLostItem(currentUser.UserId);
        if (newLostItem == null)
        {
            snackbarService.Show(
                "Ошибка",
                "Заполните все обязательные поля",
                ControlAppearance.Danger
            );
            return;
        }

        try
        {
            var itemId = await lostItemRepository.AddAsync(newLostItem);
            var createdItem = await lostItemRepository.GetByIdAsync(itemId) ?? newLostItem;
            LostItems.Insert(0, createdItem);
            ItemsCountText = $"Найдено предметов: {LostItems.Count}";

            snackbarService.Show(
                "Успех",
                "Потерянный предмет успешно добавлен",
                ControlAppearance.Success
            );

            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "CreateLostItem",
                    Details = $"Добавлен новый потерянный предмет [{newLostItem.ItemName}]",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось добавить предмет: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task UpdateLostItem(DataGridCellEditEndingEventArgs e)
    {
        if (App.CurrentUser?.RoleId == 2)
        {
            snackbarService.Show(
                "Доступ запрещен",
                "У вас нет прав на изменение данных",
                ControlAppearance.Caution
            );

            var items = await lostItemRepository.GetAllAsync();
            LostItems = new ObservableCollection<LostItem>(items);
            return;
        }

        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is LostItem item)
            {
                await UpdateLostItemAsync(item);
            }
        }
    }

    public async Task UpdateLostItemAsync(LostItem item)
    {
        try
        {
            var currentItem = await lostItemRepository.GetByIdAsync(item.ItemId);
            if (currentItem == null)
            {
                snackbarService.Show("Ошибка", "Предмет не найден", ControlAppearance.Danger);
                return;
            }

            var result = await lostItemRepository.UpdateAsync(item);

            if (result)
            {
                snackbarService.Show(
                    "Успех",
                    "Данные предмета обновлены",
                    ControlAppearance.Success
                );

                if (LostItems.FirstOrDefault(i => i.ItemId == item.ItemId) is { } updatedItem)
                {
                    var index = LostItems.IndexOf(updatedItem);
                    if (index >= 0)
                    {
                        LostItems[index] = updatedItem;
                    }
                }

                await logRepository.AddAsync(
                    new ActionLog
                    {
                        ActionType = "UpdateLostItem",
                        Details = $"Обновлен предмет: {item.ItemName}",
                    }
                );
            }
            else
            {
                snackbarService.Show(
                    "Ошибка",
                    "Не удалось обновить предмет",
                    ControlAppearance.Danger
                );
            }
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось обновить предмет: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task DeleteLostItem(LostItem item)
    {
        if (App.CurrentUser?.RoleId == 2)
        {
            snackbarService.Show(
                "Доступ запрещен",
                "У вас нет прав на удаление предметов",
                ControlAppearance.Caution
            );
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Подтверждение удаления",
            Content = $"Вы действительно хотите удалить предмет {item.ItemName}?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            await lostItemRepository.DeleteAsync(item.ItemId);
            LostItems.Remove(item);
            ItemsCountText = $"Найдено предметов: {LostItems.Count}";
            snackbarService.Show("Успех", "Предмет успешно удален", ControlAppearance.Success);
            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "RemoveLostItem",
                    Details = "Потерянный предмет удален",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось удалить предмет: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }
}
