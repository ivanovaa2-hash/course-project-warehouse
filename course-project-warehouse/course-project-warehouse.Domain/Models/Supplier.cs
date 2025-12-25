using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Models
{
    public class Supplier
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public List<Shipment> Shipments { get; set; } = new();
    }
}
