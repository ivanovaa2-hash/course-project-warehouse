using System;
using System.Collections.Generic;

namespace course_project_warehouse.UI.Model;

public partial class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public int? Quantity { get; set; }

    public int? MinStock { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Manufacturer { get; set; }

    public string? Model { get; set; }

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
