using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Data.InMemory;
using course_project_warehouse.Data.SqlServer;
using course_project_warehouse.Domain.Models;
using course_project_warehouse.Services;
using course_project_warehouse.UI.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace course_project_warehouse.UI
{
    public partial class MainWindow : Window
    {

        private readonly IProductRepository _productRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IPaymentRepository _paymentRepository;

        private List<Product> _allProducts;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Подключение к БД
                var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=WarehouseDB_Ivanov;Integrated Security=True;TrustServerCertificate=True;");
                var dbContext = new WarehouseDbContext(optionsBuilder.Options);

                _productRepository = new Data.SqlServer.ProductRepository(dbContext);
                _supplierRepository = new Data.SqlServer.SupplierRepository(dbContext);
                _shipmentRepository = new Data.SqlServer.ShipmentRepository(dbContext, _productRepository, _supplierRepository);
                _saleRepository = new Data.SqlServer.SaleRepository(dbContext, _productRepository);
                _paymentRepository = new Data.SqlServer.PaymentRepository(dbContext); // SqlServer-реализация
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}. Запустите проект с работающим LocalDB или исправьте строку подключения.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // Временно закомментировано для создания миграции
                // throw; // или Close();
                this.Close(); // закрываем приложение, чтобы не падало
            }

            LoadProducts();
            LoadCategories();
            CheckLowStockNotifications();
        }
        private void LoadProducts()
        {
            _allProducts = _productRepository.GetAll();
            ProductsDataGrid.ItemsSource = _allProducts;
            UpdateVisuals();
            CheckLowStockNotifications(); // обновляем уведомления после загрузки
        }

        private void LoadCategories()
        {
            // Статический список категорий из ТЗ (электроника)
            var categories = new List<string>
            {
                "Процессоры",
                "Видеокарты",
                "Оперативная память",
                "Материнские платы",
                "Жёсткие диски / SSD",
                "Корпуса",
                "Блоки питания",
                "Кулеры / системы охлаждения",
                "Другое"
            };

            CategoryFilterComboBox.Items.Clear();
            CategoryFilterComboBox.Items.Add("Все категории");

            // Добавляем только категории из списка ТЗ
            foreach (var category in categories)
            {
                CategoryFilterComboBox.Items.Add(category);
            }

            CategoryFilterComboBox.SelectedIndex = 0;
        }

        private void UpdateVisuals()
        {
            // Визуальное выделение товаров с низким запасом
            foreach (var item in ProductsDataGrid.Items)
            {
                if (item is Product product && product.Quantity <= product.MinStock)
                {
                    var row = ProductsDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        row.Background = System.Windows.Media.Brushes.LightSalmon;
                    }
                }
            }
        }

        private void CheckLowStockNotifications()
        {
            var lowStockProducts = _allProducts
                .Where(p => p.Quantity <= p.MinStock)
                .ToList();

            if (lowStockProducts.Any())
            {
                var productNames = string.Join("\n", lowStockProducts.Select(p => $"• {p.Name} ({p.Quantity} из {p.MinStock})"));
                MessageBox.Show($"Внимание! Низкий запас у товаров:\n{productNames}",
                    "Уведомление о запасах", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allProducts == null) return;

            var filtered = _allProducts.AsEnumerable();

            // Фильтр по тексту поиска
            var searchText = SearchTextBox.Text?.ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    p.Manufacturer.ToLower().Contains(searchText) ||
                    p.Model.ToLower().Contains(searchText));
            }

            // Фильтр по категории
            var selectedCategory = CategoryFilterComboBox.SelectedItem?.ToString();
            if (selectedCategory != null && selectedCategory != "Все категории")
            {
                filtered = filtered.Where(p => p.Category == selectedCategory);
            }

            // Фильтр по производителю
            var manufacturerText = ManufacturerFilterTextBox.Text?.ToLower();
            if (!string.IsNullOrWhiteSpace(manufacturerText))
            {
                filtered = filtered.Where(p =>
                    p.Manufacturer.ToLower().Contains(manufacturerText));
            }

            ProductsDataGrid.ItemsSource = filtered.ToList();
            UpdateVisuals();
        }

        //private void AddProduct_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var editWindow = new ProductEditWindow( _productRepository);
        //        if (editWindow.ShowDialog() == true)
        //        {
        //            LoadProducts();
        //            LoadCategories();
        //            CheckLowStockNotifications();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}", "Ошибка",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private void EditProduct_Click(object sender, RoutedEventArgs e)
        //{
        //    var selectedProduct = ProductsDataGrid.SelectedItem as Product;
        //    if (selectedProduct == null)
        //    {
        //        MessageBox.Show("Выберите товар для редактирования", "Информация",
        //            MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    try
        //    {
        //        var editWindow = new ProductEditWindow(_productRepository, selectedProduct);
        //        if (editWindow.ShowDialog() == true)
        //        {
        //            LoadProducts();
        //            CheckLowStockNotifications();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при редактировании товара: {ex.Message}", "Ошибка",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = ProductsDataGrid.SelectedItem as Product;
            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Удалить товар '{selectedProduct.Name}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _productRepository.Delete(selectedProduct.Id);
                    LoadProducts();
                    LoadCategories();
                    MessageBox.Show("Товар успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //private void AddShipment_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var selectedProduct = ProductsDataGrid.SelectedItem as Product;
        //        // Убрал параметр selectedProduct, так как конструктор ShipmentWindow не принимает его
        //        var shipmentWindow = new ShipmentWindow(_productRepository, _supplierRepository, _shipmentRepository);

        //        if (shipmentWindow.ShowDialog() == true)
        //        {
        //            LoadProducts();
        //            CheckLowStockNotifications();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при добавлении поставки: {ex.Message}", "Ошибка",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем сервис статистики, используя существующие репозитории
                var statisticsService = new StatisticsService(_productRepository, _saleRepository, _shipmentRepository);

                // Передаем сервис статистики в конструктор ReportWindow
                var reportWindow = new ReportWindow(statisticsService);
                reportWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии отчетов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var paymentWindow = new PaymentWindow(
                    _productRepository,
                    _supplierRepository,
                    _shipmentRepository,
                    _paymentRepository); // ← Передаём репозиторий

                paymentWindow.ShowDialog();

                // После закрытия можно обновить список поставок, если нужно
                // LoadProducts(); // опционально
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна оплат: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //private void ProductsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    EditProduct_Click(sender, e);
        //}
    }
}