using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Statistics
{
    public record MovementStatisticItem
    {
        public required DateOnly Date { get; set; }
        public required MovementType Type { get; set; }
        public required int Quantity { get; set; }
        public required decimal Amount { get; set; }
    }

    public enum MovementType { Sales, Writeoff, Incoming }
}
