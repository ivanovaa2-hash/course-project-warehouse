using System;
using System.Collections.Generic;

namespace course_project_warehouse.UI.Model;

public partial class Sale
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitSalePrice { get; set; }

    public string Reason { get; set; } = null!;

    public DateOnly SaleDate { get; set; }

    public decimal? TotalRevenue { get; set; }

    public virtual Product Product { get; set; } = null!;
}
