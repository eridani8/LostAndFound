using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace LostAndFound.ViewModels;

public partial class CategoriesViewModel(
    CategoryRepository categoryRepository,
    ISnackbarService snackbarService,
    IContentDialogService dialogService,
    ActionLogRepository logRepository
) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task WindowLoaded()
    {
        await LoadCategoriesAsync();
    }

    [RelayCommand]
    private async Task DeleteCategory(Category category)
    {
        var dialog = new ContentDialog
        {
            Title = "Подтверждение удаления",
            Content = $"Вы действительно хотите удалить категорию {category.CategoryName}?",
            PrimaryButtonText = "Удалить",
            CloseButtonText = "Отмена",
            DefaultButton = ContentDialogButton.Close,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        try
        {
            await categoryRepository.DeleteAsync(category.CategoryId);
            Categories.Remove(category);
            snackbarService.Show("Успех", "Категория успешно удалена", ControlAppearance.Success);
            await logRepository.AddAsync(
                new ActionLog { ActionType = "RemoveCategory", Details = "Категория удалена" }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось удалить категорию: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }
}
