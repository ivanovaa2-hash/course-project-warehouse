using course_project_warehouse.Data.InMemory;
using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace course_project_warehouse.UI.Windows
{
    public partial class PaymentWindow : Window
    {
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IPaymentRepository _paymentRepository;

        private ObservableCollection<Supplier> _suppliers;
        private ObservableCollection<Product> _products; // Правильный тип!
        private ObservableCollection<ShipmentViewModel> _shipments;

        public PaymentWindow(
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            IShipmentRepository shipmentRepository,
            IPaymentRepository paymentRepository)
        {
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _shipmentRepository = shipmentRepository;
            _paymentRepository = paymentRepository;

            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            // Загрузка поставщиков и продуктов один раз
            _suppliers = new ObservableCollection<Supplier>(_supplierRepository.GetAll());
            _products = new ObservableCollection<Product>(_productRepository.GetAll());

            SupplierComboBox.ItemsSource = _suppliers;

            // Способ оплаты
            if (PaymentMethodComboBox.Items.Count > 0)
                PaymentMethodComboBox.SelectedIndex = 0;

            // Дата по умолчанию
            PaymentDatePicker.SelectedDate = DateTime.Today;

            // Загрузка поставок
            LoadShipments();
        }

        private void LoadShipments()
        {
            var allShipments = _shipmentRepository.GetAll(new ShipmentFilter());

            _shipments = new ObservableCollection<ShipmentViewModel>(
                allShipments.Select(shipment =>
                {
                    var product = _products.FirstOrDefault(p => p.Id == shipment.ProductId);
                    var supplier = _suppliers.FirstOrDefault(s => s.Id == shipment.SupplierId);
                    var paid = _paymentRepository.GetTotalPaidForShipment(shipment.Id);
                    var remaining = shipment.TotalCost - paid;

                    return new ShipmentViewModel
                    {
                        Id = shipment.Id,
                        DisplayName = $"{product?.Name ?? "Неизвестный товар"} " +
                                      $"от {supplier?.Name ?? "Неизвестный поставщик"} " +
                                      $"- Всего: {shipment.TotalCost:C} " +
                                      $"(оплачено: {paid:C}, остаток: {remaining:C}) " +
                                      $"[{shipment.ShipmentDate:dd.MM.yyyy}]",
                        ShipmentDate = shipment.ShipmentDate,
                        TotalCost = shipment.TotalCost,
                        PaidAmount = paid,
                        SupplierId = shipment.SupplierId
                    };
                })
            );

            ShipmentComboBox.ItemsSource = _shipments;
        }

        private void SupplierComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupplierComboBox.SelectedItem is Supplier selectedSupplier)
            {
                var filtered = _shipments.Where(s => s.SupplierId == selectedSupplier.Id).ToList();
                ShipmentComboBox.ItemsSource = filtered;

                if (filtered.Any())
                    ShipmentComboBox.SelectedIndex = 0;
            }
            else
            {
                ShipmentComboBox.ItemsSource = _shipments; // Все поставки
            }

            UpdateRemainingAmount();
        }

        private void PaymentAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRemainingAmount();
        }

        private void UpdateRemainingAmount()
        {
            if (ShipmentComboBox.SelectedItem is ShipmentViewModel selected &&
                decimal.TryParse(PaymentAmountTextBox.Text, out decimal amount))
            {
                var remaining = selected.TotalCost - selected.PaidAmount - amount;
                RemainingAmountTextBox.Text = remaining.ToString("F2");
            }
            else if (ShipmentComboBox.SelectedItem is ShipmentViewModel sel)
            {
                var remaining = sel.TotalCost - sel.PaidAmount;
                RemainingAmountTextBox.Text = remaining.ToString("F2");
            }
            else
            {
                RemainingAmountTextBox.Text = "0.00";
            }
        }

        private void PayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm()) return;

                var selectedShipment = (ShipmentViewModel)ShipmentComboBox.SelectedItem;

                var payment = new Payment
                {
                    ShipmentId = selectedShipment.Id,
                    Amount = decimal.Parse(PaymentAmountTextBox.Text),
                    PaymentDate = PaymentDatePicker.SelectedDate.Value,
                    PaymentMethod = ((ComboBoxItem)PaymentMethodComboBox.SelectedItem).Content.ToString()
                };

                _paymentRepository.Add(payment); // ← Реальное сохранение!

                MessageBox.Show($"Оплата на сумму {payment.Amount:C} успешно проведена и сохранена!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проведении оплаты: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ShipmentComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставку для оплаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(PaymentAmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму оплаты (больше 0)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (PaymentMethodComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите способ оплаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!PaymentDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату оплаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var selected = (ShipmentViewModel)ShipmentComboBox.SelectedItem;
            var currentRemaining = selected.TotalCost - selected.PaidAmount;

            if (amount > currentRemaining)
            {
                MessageBox.Show($"Сумма оплаты не может превышать остаток ({currentRemaining:C})", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.Text[0]) || e.Text[0] == '.' || e.Text[0] == ',');
        }

        // Вспомогательный класс
        public class ShipmentViewModel
        {
            public Guid Id { get; set; }
            public string DisplayName { get; set; } = string.Empty;
            public DateOnly ShipmentDate { get; set; }
            public decimal TotalCost { get; set; }
            public decimal PaidAmount { get; set; }
            public Guid SupplierId { get; set; }
        }
    }
}