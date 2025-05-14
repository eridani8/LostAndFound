using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using LostAndFound.Data;
using LostAndFound.Models;

namespace LostAndFound.ViewModels;

public partial class ChartsViewModel(LostItemRepository lostItemRepository, ItemReturnRepository itemReturnRepository)
    : ObservableObject
{
    private readonly Brush[] _pieChartColors =
    [
        new SolidColorBrush(Color.FromRgb(33, 150, 243)),
        new SolidColorBrush(Color.FromRgb(76, 175, 80)),
        new SolidColorBrush(Color.FromRgb(255, 152, 0)),
        new SolidColorBrush(Color.FromRgb(233, 30, 99)),
        new SolidColorBrush(Color.FromRgb(156, 39, 176)),
        new SolidColorBrush(Color.FromRgb(0, 188, 212)),
        new SolidColorBrush(Color.FromRgb(244, 67, 54))
    ];

    [ObservableProperty] private SeriesCollection _pieChartSeries = [];

    [ObservableProperty] private SeriesCollection _barChartSeries = [];

    [ObservableProperty] private List<string> _pieChartLabels = [];

    [ObservableProperty] private List<string> _barChartLabels = [];

    [ObservableProperty] private Func<double, string> _formatter = value => value.ToString("N0");

    [RelayCommand]
    private async Task WindowLoaded()
    {
        List<LostItem> items = []; // (await lostItemRepository.GetAllAsync()).ToList();
        if (items.Count == 0)
        {
            CreateSampleData();
            return;
        }

        var categoriesCount = items
            .GroupBy(i => i.Category?.CategoryName ?? "Без категории")
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        PieChartSeries.Clear();
        PieChartLabels.Clear();


        for (var i = 0; i < categoriesCount.Count; i++)
        {
            var category = categoriesCount[i];
            var colorIndex = i % _pieChartColors.Length;

            PieChartSeries.Add(
                new PieSeries
                {
                    Title = category.Category,
                    Values = new ChartValues<int> { category.Count },
                    DataLabels = true,
                    Fill = _pieChartColors[colorIndex],
                    Stroke = _pieChartColors[colorIndex],
                    StrokeThickness = 0,
                }
            );
            PieChartLabels.Add(category.Category);
        }

        var currentYear = DateTime.Now.Year;
        var monthlyItems = items
            .Where(i => i.FoundDate.Year == currentYear)
            .GroupBy(i => i.FoundDate.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToList();

        var monthNames = new[]
        {
            "Янв",
            "Фев",
            "Мар",
            "Апр",
            "Май",
            "Июн",
            "Июл",
            "Авг",
            "Сен",
            "Окт",
            "Ноя",
            "Дек",
        };

        BarChartLabels.Clear();
        var monthlyValues = new ChartValues<int>();

        for (var month = 1; month <= 12; month++)
        {
            var monthData = monthlyItems.FirstOrDefault(m => m.Month == month);
            monthlyValues.Add(monthData?.Count ?? 0);
            BarChartLabels.Add(monthNames[month - 1]);
        }

        BarChartSeries.Clear();
        BarChartSeries.Add(
            new ColumnSeries
            {
                Title = "Найденные предметы",
                Values = monthlyValues,
                Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                StrokeThickness = 0,
                DataLabels = true,
            }
        );
    }

    private void CreateSampleData()
    {
        PieChartSeries.Clear();
        PieChartLabels.Clear();

        var sampleCategories = new[]
        {
            new { Name = "Электроника", Count = 35 },
            new { Name = "Одежда", Count = 25 },
            new { Name = "Документы", Count = 20 },
            new { Name = "Ключи", Count = 15 },
            new { Name = "Другое", Count = 10 },
        };

        for (var i = 0; i < sampleCategories.Length; i++)
        {
            var category = sampleCategories[i];

            PieChartSeries.Add(
                new PieSeries
                {
                    Title = category.Name,
                    Values = new ChartValues<int> { category.Count },
                    DataLabels = true,
                    Fill = _pieChartColors[i % _pieChartColors.Length],
                    Stroke = _pieChartColors[i % _pieChartColors.Length],
                    StrokeThickness = 0,
                }
            );
            PieChartLabels.Add(category.Name);
        }

        BarChartSeries.Clear();
        BarChartLabels.Clear();

        var monthNames = new[]
        {
            "Янв",
            "Фев",
            "Мар",
            "Апр",
            "Май",
            "Июн",
            "Июл",
            "Авг",
            "Сен",
            "Окт",
            "Ноя",
            "Дек",
        };

        foreach (var month in monthNames)
        {
            BarChartLabels.Add(month);
        }

        var monthlyValues = new ChartValues<int> { 5, 8, 12, 15, 10, 8, 6, 7, 9, 14, 18, 20 };

        BarChartSeries.Add(
            new ColumnSeries
            {
                Title = "Найденные предметы",
                Values = monthlyValues,
                Fill = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                StrokeThickness = 0,
                DataLabels = true,
            }
        );
    }
}