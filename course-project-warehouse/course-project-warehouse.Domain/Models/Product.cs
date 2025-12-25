using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Другое";
        public int Quantity { get; set; } = 0;
        public int MinStock { get; set; } = 0;
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;

        public List<Shipment> Shipments { get; set; } = new();
        public List<Sale> Sales { get; set; } = new();
    }
}
