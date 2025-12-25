using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Models
{
    public class Shipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid ProductId { get; set; }
        public required Guid SupplierId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice { get; set; }
        public required DateOnly ShipmentDate { get; set; }
        public decimal TotalCost => Quantity * UnitPrice;
        public string DocumentNumber { get; set; } = string.Empty;
        public Product? Product { get; set; }
        public Supplier? Supplier { get; set; }
        public List<Payment> Payments { get; set; } = new();
    }
}
