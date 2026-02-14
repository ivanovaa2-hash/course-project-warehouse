using System;
using System.Collections.Generic;

namespace course_project_warehouse.UI.Model;

public partial class Supplier
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
}
