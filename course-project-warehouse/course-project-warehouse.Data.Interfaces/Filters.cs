using course_project_warehouse.Domain.Models;
using course_project_warehouse.Domain.Statistics;
using System.Diagnostics;
namespace course_project_warehouse.Data.Interfaces;


    public record CommonFilter
    {
        public static CommonFilter Empty => new();
        public DateOnly? StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
    }

    public record SalesFilter : CommonFilter
    {
        public Guid? ProductId { get; init; }
        public ReasonType? Reason { get; init; }
    }

    public record ShipmentFilter : CommonFilter
    {
        public Guid? ProductId { get; init; }
        public Guid? SupplierId { get; init; }
    }
