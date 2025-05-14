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
    private async Task WindowLoaded()
    {
        var categories = await categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(categories);
    }

    [RelayCommand]
    private async Task UpdateCategory(DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.Row.Item is Category category)
            {
                await UpdateCategoryAsync(category);
            }
        }
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        try
        {
            var currentCategory = await categoryRepository.GetByIdAsync(category.CategoryId);
            if (currentCategory == null)
            {
                snackbarService.Show("Ошибка", "Категория не найдена", ControlAppearance.Danger);
                return;
            }

            var result = await categoryRepository.UpdateAsync(category);

            if (result)
            {
                snackbarService.Show(
                    "Успех",
                    "Данные категории обновлены",
                    ControlAppearance.Success
                );

                if (
                    Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId) is
                    { } updatedCategory
                )
                {
                    var index = Categories.IndexOf(updatedCategory);
                    if (index >= 0)
                    {
                        Categories[index] = updatedCategory;
                    }
                }

                await logRepository.AddAsync(
                    new ActionLog
                    {
                        ActionType = "UpdateCategory",
                        Details = $"Обновлена категория: {category.CategoryName}",
                    }
                );
            }
            else
            {
                snackbarService.Show(
                    "Ошибка",
                    "Не удалось обновить категорию",
                    ControlAppearance.Danger
                );
            }
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось обновить категорию: {ex.Message}",
                ControlAppearance.Danger
            );
        }
    }

    [RelayCommand]
    private async Task CreateCategory()
    {
        var createCategoryControl = new CreateCategoryDialog();

        var dialog = new ContentDialog
        {
            Title = "Создание категории",
            Content = createCategoryControl,
            CloseButtonText = "Отмена",
            PrimaryButtonText = "Создать",
            DefaultButton = ContentDialogButton.Primary,
        };

        var result = await dialogService.ShowAsync(dialog, CancellationToken.None);

        if (result != ContentDialogResult.Primary)
            return;

        var newCategory = createCategoryControl.CreateCategory();
        if (newCategory == null)
        {
            snackbarService.Show(
                "Ошибка",
                "Заполните название категории",
                ControlAppearance.Danger
            );
            return;
        }

        try
        {
            var categoryId = await categoryRepository.AddAsync(newCategory);
            var createdCategory = await categoryRepository.GetByIdAsync(categoryId) ?? newCategory;
            createdCategory.CategoryId = categoryId;
            Categories.Insert(0, createdCategory);

            snackbarService.Show("Успех", "Категория успешно создана", ControlAppearance.Success);

            await logRepository.AddAsync(
                new ActionLog
                {
                    ActionType = "CreateCategory",
                    Details = $"Создана новая категория [{newCategory.CategoryName}]",
                }
            );
        }
        catch (Exception ex)
        {
            snackbarService.Show(
                "Ошибка",
                $"Не удалось создать категорию: {ex.Message}",
                ControlAppearance.Danger
            );
        }
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
