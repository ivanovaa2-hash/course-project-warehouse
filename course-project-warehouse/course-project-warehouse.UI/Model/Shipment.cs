using System;
using System.Collections.Generic;

namespace course_project_warehouse.UI.Model;

public partial class Shipment
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid SupplierId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public DateOnly ShipmentDate { get; set; }

    public string? DocumentNumber { get; set; }

    public decimal? TotalCost { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product Product { get; set; } = null!;

    public virtual Supplier Supplier { get; set; } = null!;
}
