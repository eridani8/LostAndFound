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
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<LostItem> _lostItems = [];

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var items = await lostItemRepository.GetAllAsync();
        LostItems = new ObservableCollection<LostItem>(items);
    }

    [RelayCommand]
    private async Task CreateLostItem()
    {
        if (App.CurrentUser is not { } currentUser)
            return;

        var createLostItemControl = new CreateLostItemDialog();

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
