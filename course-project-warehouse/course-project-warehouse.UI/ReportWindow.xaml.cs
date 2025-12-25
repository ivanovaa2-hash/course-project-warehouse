using course_project_warehouse.Data.InMemory;
using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Statistics;
using course_project_warehouse.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Windows;

namespace course_project_warehouse.UI
{
    public partial class ReportWindow : Window
    {
        private readonly StatisticsService _statisticsService;

        // Конструктор, принимающий StatisticsService
        public ReportWindow(StatisticsService statisticsService)
        {
            InitializeComponent();
            _statisticsService = statisticsService;

            // Загружаем всё при открытии
            LoadAllCharts();
            LoadProductsList();
        }

        // Конструктор без параметров для дизайнера (но он не будет использоваться)
        private ReportWindow()
        {
            InitializeComponent();
        }

        private void LoadAllCharts()
        {
            LoadStockChart();
            LoadSalesChart();
            LoadProfitChart();
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadAllCharts();
            if (ProductComboBox.SelectedItem != null)
                RefreshMovement_Click(sender, e);
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            LoadAllCharts();
            MovementGrid.ItemsSource = null;
        }

        private CommonFilter GetFilter()
        {
            DateOnly? start = StartDatePicker.SelectedDate.HasValue
                ? DateOnly.FromDateTime(StartDatePicker.SelectedDate.Value)
                : null;

            DateOnly? end = EndDatePicker.SelectedDate.HasValue
                ? DateOnly.FromDateTime(EndDatePicker.SelectedDate.Value)
                : null;

            return new CommonFilter { StartDate = start, EndDate = end };
        }

        private void LoadStockChart()
        {
            var data = _statisticsService.GetCurrentStock();

            var model = new PlotModel { Title = "Текущие остатки товаров" };

            var barSeries = new BarSeries { FillColor = OxyColors.SteelBlue };
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };

            foreach (var item in data)
            {
                barSeries.Items.Add(new BarItem { Value = item.Quantity });
                categoryAxis.Labels.Add(item.ProductName);
            }

            model.Series.Add(barSeries);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Количество" });

            StockPlot.Model = model;
        }

        private void LoadSalesChart()
        {
            var filter = GetFilter();
            var data = _statisticsService.GetSalesByMonth(filter);

            var model = new PlotModel { Title = "Продажи по месяцам" };

            var lineSeries = new LineSeries { Color = OxyColors.Orange, MarkerType = MarkerType.Circle };
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };

            for (int i = 0; i < data.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, (double)data[i].Value));
                categoryAxis.Labels.Add(data[i].GetMonthName());
            }

            model.Series.Add(lineSeries);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Количество" });

            SalesPlot.Model = model;
        }

        private void LoadProfitChart()
        {
            var filter = GetFilter();
            var data = _statisticsService.GetProfitByMonth(filter);

            var model = new PlotModel { Title = "Прибыль по месяцам" };

            var lineSeries = new LineSeries { Color = OxyColors.Green, MarkerType = MarkerType.Circle };
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom };

            for (int i = 0; i < data.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i, (double)data[i].Value));
                categoryAxis.Labels.Add(data[i].GetMonthName());
            }

            model.Series.Add(lineSeries);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Прибыль (руб.)" });

            ProfitPlot.Model = model;
        }

        private void LoadProductsList()
        {
            var products = _statisticsService.GetCurrentStock();
            ProductComboBox.ItemsSource = products;
            if (products.Count > 0)
                ProductComboBox.SelectedIndex = 0;
        }

        private void RefreshMovement_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is StockStatisticItem selected)
            {
                var filter = GetFilter();
                var movement = _statisticsService.GetProductMovement(selected.ProductId, filter);
                MovementGrid.ItemsSource = movement;
            }
        }
    }
}