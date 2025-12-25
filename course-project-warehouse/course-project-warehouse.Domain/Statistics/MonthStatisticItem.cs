using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Statistics
{
    public record MonthStatisticItem
    {
        public required int Year { get; set; }
        public required int Month { get; set; }
        public required decimal Value { get; set; }

        public string GetMonthName() => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}
