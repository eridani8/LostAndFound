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

public partial class StorageLocationsViewModel(
    StorageLocationRepository storageLocationRepository,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<StorageLocation> _storageLocations = [];

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var items = await storageLocationRepository.GetAllAsync();
        StorageLocations = new ObservableCollection<StorageLocation>(items);
    }

    [RelayCommand]
    private async Task CreateStorageLocation()
    {
        var createDialog = new CreateStorageLocationDialog();
        var dialog = new ContentDialog
        {
            Title = "Добавить место хранения",
            Content = createDialog,
            CloseButtonText = "Отмена",
            PrimaryButtonText = "Создать",
            DefaultButton = ContentDialogButton.Primary,
        };
        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);
        if (result != ContentDialogResult.Primary)
            return;
        var newLocation = createDialog.CreateStorageLocation();
        if (newLocation == null)
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
            var id = await storageLocationRepository.AddAsync(newLocation);
            var created = await storageLocationRepository.GetByIdAsync(id) ?? newLocation;
            StorageLocations.Insert(0, created);
            snackbarService.Show("Успех", "Место хранения добавлено", ControlAppearance.Success);
            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "CreateStorageLocation",
                    Details = $"Добавлено место хранения [{newLocation.LocationName}]",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось добавить: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task UpdateStorageLocation(DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is StorageLocation location)
            {
                await UpdateStorageLocationAsync(location);
            }
        }
    }

    public async Task UpdateStorageLocationAsync(StorageLocation location)
    {
        try
        {
            var current = await storageLocationRepository.GetByIdAsync(location.StorageLocationId);
            if (current == null)
            {
                snackbarService.Show(
                    "Ошибка",
                    "Место хранения не найдено",
                    ControlAppearance.Danger
                );
                return;
            }
            var result = await storageLocationRepository.UpdateAsync(location);
            if (result)
            {
                snackbarService.Show("Успех", "Данные обновлены", ControlAppearance.Success);
                if (
                    StorageLocations.FirstOrDefault(x =>
                        x.StorageLocationId == location.StorageLocationId
                    ) is
                    { } updated
                )
                {
                    var index = StorageLocations.IndexOf(updated);
                    if (index >= 0)
                        StorageLocations[index] = updated;
                }
                await logRepository.AddAsync(
                    new ActionLog
                    {
                        ActionType = "UpdateStorageLocation",
                        Details = $"Обновлено место хранения: {location.LocationName}",
                    }
                );
            }
            else
            {
                snackbarService.Show("Ошибка", "Не удалось обновить", ControlAppearance.Danger);
            }
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось обновить: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task DeleteStorageLocation(StorageLocation location)
    {
        var dialog = new ContentDialog
        {
            Title = "Подтверждение удаления",
            Content = $"Вы действительно хотите удалить место хранения {location.LocationName}?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close,
        };
        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);
        if (result != ContentDialogResult.Primary)
            return;
        try
        {
            await storageLocationRepository.DeleteAsync(location.StorageLocationId);
            StorageLocations.Remove(location);
            snackbarService.Show("Успех", "Место хранения удалено", ControlAppearance.Success);
            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "RemoveStorageLocation",
                    Details = "Место хранения удалено",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось удалить: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }
}
