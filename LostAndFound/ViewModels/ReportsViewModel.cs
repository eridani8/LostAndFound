using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.Win32;

namespace LostAndFound.ViewModels;

public partial class ReportsViewModel(
    LostItemRepository lostItemRepository,
    ItemReturnRepository itemReturnRepository,
    CategoryRepository categoryRepository
) : ObservableObject
{
    [ObservableProperty] private string _selectedReportType = "Потерянные предметы";

    [ObservableProperty] private DateTime _startDate = DateTime.Today.AddYears(-10);

    [ObservableProperty] private DateTime _endDate = DateTime.Today;

    [ObservableProperty] private ObservableCollection<object> _reportResults = [];

    [ObservableProperty] private ObservableCollection<Category> _categories = [];

    [ObservableProperty] private Category? _selectedCategory;

    [ObservableProperty] private string _selectedFileFormat = "Текстовый файл";

    public string[] ReportTypes { get; } =
        ["Потерянные предметы", "Возвращенные предметы", "По типу вещи"];

    public string[] FileFormats { get; } = ["Текстовый файл", "Word", "Excel"];

    [RelayCommand]
    private async Task LoadCategories()
    {
        var allCategories = await categoryRepository.GetAllAsync();
        Categories = new ObservableCollection<Category>(allCategories);
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        switch (SelectedReportType)
        {
            case "Потерянные предметы":
            {
                var all = await lostItemRepository.GetAllAsync();
                var filtered = all.Where(x => x.FoundDate >= StartDate && x.FoundDate <= EndDate)
                    .ToList();
                ReportResults = new ObservableCollection<object>(filtered);
                break;
            }
            case "Возвращенные предметы":
            {
                var all = await itemReturnRepository.GetAllAsync();
                var filtered = all.Where(x => x.ReturnDate >= StartDate && x.ReturnDate <= EndDate)
                    .ToList();
                ReportResults = new ObservableCollection<object>(filtered);
                break;
            }
            case "По типу вещи":
            {
                if (SelectedCategory == null)
                {
                    if (Categories.Count == 0)
                    {
                        await LoadCategories();
                    }

                    return;
                }

                var all = await lostItemRepository.GetAllAsync();
                var filtered = all.Where(x =>
                        x.CategoryId == SelectedCategory.CategoryId
                        && x.FoundDate >= StartDate
                        && x.FoundDate <= EndDate
                    )
                    .ToList();

                ReportResults = new ObservableCollection<object>(filtered);
                break;
            }
        }
    }

    [RelayCommand]
    private async Task SaveReport()
    {
        if (ReportResults.Count == 0)
            return;

        var reportTitle = SelectedReportType;
        if (SelectedReportType == "По типу вещи" && SelectedCategory != null)
        {
            reportTitle = $"Отчет по типу вещи: {SelectedCategory.CategoryName}";
        }

        string filter;
        string extension;

        switch (SelectedFileFormat)
        {
            case "Word":
                filter = "Документ Word (*.docx)|*.docx";
                extension = ".docx";
                break;
            case "Excel":
                filter = "Таблица Excel (*.xlsx)|*.xlsx";
                extension = ".xlsx";
                break;
            default:
                filter = "Текстовый файл (*.txt)|*.txt";
                extension = ".txt";
                break;
        }

        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}",
        };

        if (dialog.ShowDialog() != true)
            return;

        var path = dialog.FileName;

        switch (SelectedFileFormat)
        {
            case "Word":
                await WordDocumentService.SaveReportToWordAsync(
                    reportTitle,
                    StartDate,
                    EndDate,
                    ReportResults,
                    path
                );
                break;
            case "Excel":
                await ExcelDocumentService.SaveReportToExcelAsync(
                    reportTitle,
                    StartDate,
                    EndDate,
                    ReportResults,
                    path
                );
                break;
            default:
                await SaveToTextFile(path, reportTitle);
                break;
        }
    }

    private async Task SaveToTextFile(string path, string reportTitle)
    {
        await using var sw = new StreamWriter(path, false, Encoding.UTF8);

        await sw.WriteLineAsync(
            $"{reportTitle}{Environment.NewLine}{StartDate.ToLongDateString()} - {EndDate.ToLongDateString()}"
        );
        await sw.WriteLineAsync();

        foreach (var item in ReportResults)
        {
            switch (item)
            {
                case LostItem lostItem:
                    await sw.WriteLineAsync(
                        $"ID: {lostItem.ItemId}, Название: {lostItem.ItemName}, "
                        + $"Категория: {lostItem.Category?.CategoryName ?? "Не указана"}, "
                        + $"Найдено: {lostItem.FoundDate:dd.MM.yyyy}, "
                        + $"Место: {lostItem.FoundLocation}, "
                        + $"Статус: {lostItem.Status}"
                    );
                    break;
                case ItemReturn itemReturn:
                    await sw.WriteLineAsync(
                        $"ID: {itemReturn.ReturnId}, "
                        + $"Предмет: {itemReturn.LostItem?.ItemName ?? "Неизвестно"}, "
                        + $"Кому возвращен: {itemReturn.ReturnedTo}, "
                        + $"Дата возврата: {itemReturn.ReturnDate:dd.MM.yyyy}, "
                        + $"Контактная информация: {itemReturn.ContactInfo}"
                    );
                    break;
            }
        }
    }
}