using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LostAndFound.Data;
using LostAndFound.Models;
using Microsoft.Win32;

namespace LostAndFound.ViewModels;

public partial class ReportsViewModel(
    LostItemRepository lostItemRepository,
    ItemReturnRepository itemReturnRepository
) : ObservableObject
{
    [ObservableProperty]
    private string _selectedReportType = "Потерянные предметы";

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddYears(-10);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<object> _reportResults = [];

    public string[] ReportTypes { get; } = ["Потерянные предметы", "Возвращенные предметы"];

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
        }
    }

    [RelayCommand]
    private async Task SaveReport()
    {
        if (ReportResults.Count == 0) return;

        var dialog = new SaveFileDialog
        {
            Filter = "Текстовый файл (*.txt)|*.txt",
            FileName = $"{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}",
        };
        if (dialog.ShowDialog() != true) return;
        var path = dialog.FileName;
        await using var sw = new StreamWriter(path, false, Encoding.UTF8);

        await sw.WriteLineAsync($"{SelectedReportType}{Environment.NewLine}{StartDate.ToLongDateString()} - {EndDate.ToLongDateString()}");
        await sw.WriteLineAsync();
        
        foreach (var item in ReportResults)
        {
            switch (item)
            {
                case LostItem lostItem:
                    await sw.WriteLineAsync(lostItem.ItemName);
                    break;
                case ItemReturn itemReturn:
                    await sw.WriteLineAsync(itemReturn.LostItem.ItemName);
                    break;
            }
        }
    }
}
