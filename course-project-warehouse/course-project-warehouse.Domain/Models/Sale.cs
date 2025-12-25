using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Domain.Models
{
    public class Sale
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid ProductId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitSalePrice { get; set; }
        public required ReasonType Reason { get; set; }
        public required DateOnly SaleDate { get; set; }
        public decimal TotalRevenue => Quantity * UnitSalePrice;

        public Product? Product { get; set; }
    }

    public enum ReasonType 
    { 
        Sale, 
        Defect, 
        Return 
    }
}


