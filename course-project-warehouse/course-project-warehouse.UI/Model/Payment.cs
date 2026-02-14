using System;
using System.Collections.Generic;

namespace course_project_warehouse.UI.Model;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid ShipmentId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual Shipment Shipment { get; set; } = null!;
}
