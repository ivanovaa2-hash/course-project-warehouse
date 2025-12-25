using course_project_warehouse.Data.InMemory;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace course_project_warehouse.UI
{
    public partial class ShipmentWindow : Window  // Обратите внимание на 'partial'
    {
        private readonly ProductRepository _productRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly ShipmentRepository _shipmentRepository;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Supplier> Suppliers { get; set; }
        public ObservableCollection<ShipmentItem> ShipmentItems { get; set; }

        public ShipmentWindow(ProductRepository productRepository,
                              SupplierRepository supplierRepository,
                              ShipmentRepository shipmentRepository)
        {
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _shipmentRepository = shipmentRepository;

            InitializeComponent(); // Этот метод генерируется автоматически из XAML

            InitializeData();
        }

        private void InitializeData()
        {
            // Загрузка данных
            Products = new ObservableCollection<Product>(_productRepository.GetAll());
            Suppliers = new ObservableCollection<Supplier>(_supplierRepository.GetAll());
            ShipmentItems = new ObservableCollection<ShipmentItem>();

            // Привязка ComboBox
            SupplierComboBox.ItemsSource = Suppliers;
            if (Suppliers.Any()) SupplierComboBox.SelectedIndex = 0;

            // Установка текущей даты
            ShipmentDatePicker.SelectedDate = DateTime.Today;

            // Настройка DataGrid
            ItemsDataGrid.ItemsSource = ShipmentItems;

            // Добавление первой строки по умолчанию
            AddNewItem();
        }

        private void AddNewItem()
        {
            var newItem = new ShipmentItem
            {
                ProductId = Products.FirstOrDefault()?.Id ?? Guid.Empty,
                Products = Products
            };
            ShipmentItems.Add(newItem);
        }

        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewItem();
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is ShipmentItem item)
            {
                ShipmentItems.Remove(item);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (!ValidateForm()) return;

                // Сохранение поставок
                foreach (var item in ShipmentItems)
                {
                    var shipment = new Shipment
                    {
                        ProductId = item.ProductId,
                        SupplierId = ((Supplier)SupplierComboBox.SelectedItem).Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ShipmentDate = DateOnly.FromDateTime(ShipmentDatePicker.SelectedDate.Value),
                        DocumentNumber = DocumentNumberTextBox.Text
                    };

                    _shipmentRepository.Add(shipment);
                }

                MessageBox.Show("Поставка успешно добавлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            // Проверка поставщика
            if (SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка даты
            if (!ShipmentDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату поступления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка номера документа
            if (string.IsNullOrWhiteSpace(DocumentNumberTextBox.Text))
            {
                MessageBox.Show("Введите номер документа", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка товаров
            if (!ShipmentItems.Any())
            {
                MessageBox.Show("Добавьте хотя бы один товар", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка каждого товара
            foreach (var item in ShipmentItems)
            {
                if (item.ProductId == Guid.Empty)
                {
                    MessageBox.Show("Выберите товар для всех строк", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (item.Quantity <= 0)
                {
                    MessageBox.Show("Количество должно быть больше 0", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (item.UnitPrice <= 0)
                {
                    MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Валидация ввода чисел
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Разрешаем только цифры
                e.Handled = !char.IsDigit(e.Text[0]);
            }
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Разрешаем цифры и точку
                e.Handled = !(char.IsDigit(e.Text[0]) || e.Text[0] == '.');
            }
        }
    }

    // Класс для представления строки товара в DataGrid
    public class ShipmentItem
    {
        public Guid ProductId { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalCost => Quantity * UnitPrice;
    }
}