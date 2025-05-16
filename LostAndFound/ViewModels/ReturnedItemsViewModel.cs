using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Views.Dialogs;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LostAndFound.ViewModels;

public partial class ReturnedItemsViewModel(
    ItemReturnRepository returnRepository,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository,
    LostItemRepository lostItemRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ItemReturn> _returnedItems = [];

    [RelayCommand]
    private async Task WindowLoaded()
    {
        var items = await returnRepository.GetAllAsync();
        ReturnedItems = new ObservableCollection<ItemReturn>(items);
    }

    [RelayCommand]
    private async Task CreateItemReturn()
    {
        if (App.CurrentUser is not { } currentUser)
            return;

        var createItemReturnControl = new CreateItemReturnDialog(lostItemRepository);
        await createItemReturnControl.LoadDataAsync();

        var dialog = new ContentDialog
        {
            Title = "Возврат предмета",
            Content = createItemReturnControl,
            CloseButtonText = "Отмена",
            PrimaryButtonText = "Создать",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        var newItemReturn = createItemReturnControl.CreateItemReturn(currentUser.UserId);
        if (newItemReturn == null)
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
            var returnId = await returnRepository.AddAsync(newItemReturn);
            var createdReturn = await returnRepository.GetByIdAsync(returnId) ?? newItemReturn;
            ReturnedItems.Insert(0, createdReturn);

            snackbarService.Show(
                "Успех",
                "Возврат предмета успешно зарегистрирован",
                ControlAppearance.Success
            );

            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "RegisterReturn",
                    Details =
                        $"Возврат предмета ID={newItemReturn.ItemId} для {newItemReturn.ReturnedTo}",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось зарегистрировать возврат предмета: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task DeleteItemReturn(ItemReturn itemReturn)
    {
        var dialog = new ContentDialog
        {
            Title = "Подтверждение удаления",
            Content =
                $"Вы действительно хотите удалить запись о возврате предмета {itemReturn.LostItem?.ItemName}?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            await returnRepository.DeleteAsync(itemReturn.ReturnId);
            ReturnedItems.Remove(itemReturn);
            snackbarService.Show(
                "Успех",
                "Запись о возврате успешно удалена",
                ControlAppearance.Success
            );
            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "RemoveReturn",
                    Details = "Удаление записи о возврате предмета",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось удалить запись о возврате: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }
}
