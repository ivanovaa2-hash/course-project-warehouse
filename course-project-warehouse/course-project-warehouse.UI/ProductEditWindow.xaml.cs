using course_project_warehouse.Data.InMemory;
using course_project_warehouse.Domain.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace course_project_warehouse.UI.Windows
{
    public partial class ProductEditWindow : Window
    {
        private readonly ProductRepository _productRepository;
        private readonly Product _product;
        private readonly bool _isEditMode;

        public ProductEditWindow(ProductRepository productRepository, Product product = null)
        {
            InitializeComponent();

            _productRepository = productRepository;
            _product = product;
            _isEditMode = product != null;

            // Установка заголовка окна
            this.Title = _isEditMode ? "Редактирование товара" : "Добавление товара";

            if (_isEditMode)
            {
                LoadProductData();
            }

            // Установка начального значения категории
            if (CategoryComboBox.Items.Count > 0)
                CategoryComboBox.SelectedIndex = 0;
        }

        private void LoadProductData()
        {
            if (_product == null) return;

            NameTextBox.Text = _product.Name;
            ManufacturerTextBox.Text = _product.Manufacturer;
            ModelTextBox.Text = _product.Model;
            QuantityTextBox.Text = _product.Quantity.ToString();
            MinStockTextBox.Text = _product.MinStock.ToString();
            PurchasePriceTextBox.Text = _product.PurchasePrice.ToString("F2");
            SalePriceTextBox.Text = _product.SalePrice.ToString("F2");

            // Установка категории
            foreach (ComboBoxItem item in CategoryComboBox.Items)
            {
                if (item.Content.ToString() == _product.Category)
                {
                    CategoryComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm()) return;

                var product = new Product
                {
                    Name = NameTextBox.Text,
                    Category = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString(),
                    Manufacturer = ManufacturerTextBox.Text,
                    Model = ModelTextBox.Text,
                    Quantity = int.Parse(QuantityTextBox.Text),
                    MinStock = int.Parse(MinStockTextBox.Text),
                    PurchasePrice = decimal.Parse(PurchasePriceTextBox.Text),
                    SalePrice = decimal.Parse(SalePriceTextBox.Text)
                };

                if (_isEditMode && _product != null)
                {
                    product.Id = _product.Id;
                    product.CreatedAt = _product.CreatedAt;
                    _productRepository.Update(product);
                }
                else
                {
                    _productRepository.Add(product);
                }

                MessageBox.Show("Товар успешно сохранён!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении товара: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            // Проверка наименования
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Введите наименование товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            // Проверка категории
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            // Упрощенная проверка (работает с русской локалью)
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число, больше или равно 0)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MinStockTextBox.Text, out int minStock) || minStock < 0)
            {
                MessageBox.Show("Введите корректный минимальный запас (целое число, больше или равно 0)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                MinStockTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(PurchasePriceTextBox.Text, out decimal purchasePrice) || purchasePrice < 0)
            {
                MessageBox.Show("Введите корректную цену закупки (число, больше или равно 0)\nИспользуйте запятую для дробной части", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PurchasePriceTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(SalePriceTextBox.Text, out decimal salePrice) || salePrice < 0)
            {
                MessageBox.Show("Введите корректную цену продажи (число, больше или равно 0)\nИспользуйте запятую для дробной части", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SalePriceTextBox.Focus();
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Разрешаем цифры, точку и запятую
                e.Handled = !(char.IsDigit(e.Text[0]) || e.Text[0] == '.' || e.Text[0] == ',');
            }
        }
    }
}