using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace course_project_warehouse.Data.InMemory;
public class ShipmentRepository : IShipmentRepository
{
    private readonly List<Shipment> _shipments = new();
    private readonly ProductRepository _productRepository;
    private readonly SupplierRepository _supplierRepository;

    public ShipmentRepository(ProductRepository productRepository, SupplierRepository supplierRepository)
    {
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        SeedData();
    }

    public List<Shipment> GetAll(ShipmentFilter filter)
    {
        var result = _shipments.AsEnumerable();

        if (filter.StartDate.HasValue)
            result = result.Where(x => x.ShipmentDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            result = result.Where(x => x.ShipmentDate <= filter.EndDate.Value);
        if (filter.ProductId.HasValue)
            result = result.Where(x => x.ProductId == filter.ProductId.Value);
        if (filter.SupplierId.HasValue)
            result = result.Where(x => x.SupplierId == filter.SupplierId.Value);

        return result.ToList();
    }

    public void Add(Shipment shipment)
    {
        shipment.Id = Guid.NewGuid();
        _shipments.Add(shipment);

        // Обновляем остаток товара
        var product = _productRepository.GetById(shipment.ProductId);
        if (product != null) product.Quantity += shipment.Quantity;
    }

    private void SeedData()
    {
        var products = _productRepository.GetAll();
        var suppliers = _supplierRepository.GetAll();
        var random = new Random();

        for (int i = 0; i < 40; i++)
        {
            var shipment = new Shipment
            {
                ProductId = products[random.Next(products.Count)].Id,
                SupplierId = suppliers[random.Next(suppliers.Count)].Id,
                Quantity = random.Next(5, 50),
                UnitPrice = random.Next(200, 50000),
                ShipmentDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(0, 365)))
            };
            Add(shipment);
        }
    }
}