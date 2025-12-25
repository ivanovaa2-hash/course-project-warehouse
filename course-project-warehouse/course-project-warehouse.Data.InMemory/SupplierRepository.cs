using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Data.InMemory;
public class SupplierRepository : ISupplierRepository 
{
    private readonly List<Supplier> _suppliers = new();

    public SupplierRepository()
    {
        SeedData();
    }

    public List<Supplier> GetAll() => _suppliers.ToList();

    public Supplier? GetById(Guid id) => _suppliers.FirstOrDefault(s => s.Id == id);

    public void Add(Supplier supplier)
    {
        supplier.Id = Guid.NewGuid();
        _suppliers.Add(supplier);
    }

    private void SeedData()
    {
        var suppliers = new[]
        {
            new Supplier { Name = "ООО ТехноПоставка", Phone = "+7(495)123-45-67", Email = "sales@techno.ru" },
            new Supplier { Name = "ИП КофеОпт", Phone = "+7(812)987-65-43", Email = "info@coffeeopt.ru" },
            new Supplier { Name = "Магазин Одежды", Phone = "+7(495)555-22-11" }
        };
        foreach (var s in suppliers) Add(s);
    }
} 
