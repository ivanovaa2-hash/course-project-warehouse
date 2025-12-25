using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace course_project_warehouse.Data.InMemory;
public class SaleRepository : ISaleRepository
{
    private readonly List<Sale> _sales = new();
    private readonly ProductRepository _productRepository;

    public SaleRepository(ProductRepository productRepository)
    {
        _productRepository = productRepository;
        SeedData();
    }

    public List<Sale> GetAll(SalesFilter filter)
    {
        var result = _sales.AsEnumerable();

        if (filter.StartDate.HasValue)
            result = result.Where(x => x.SaleDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            result = result.Where(x => x.SaleDate <= filter.EndDate.Value);
        if (filter.ProductId.HasValue)
            result = result.Where(x => x.ProductId == filter.ProductId.Value);
        if (filter.Reason.HasValue)
            result = result.Where(x => x.Reason == filter.Reason.Value);

        return result.ToList();
    }

    public void Add(Sale sale)
    {
        sale.Id = Guid.NewGuid();
        _sales.Add(sale);

        // Обновляем остаток товара
        var product = _productRepository.GetById(sale.ProductId);
        if (product != null && product.Quantity >= sale.Quantity)
            product.Quantity -= sale.Quantity;
    }

    private void SeedData()
    {
        var products = _productRepository.GetAll();
        var reasons = Enum.GetValues<ReasonType>();
        var random = new Random();

        for (int i = 0; i < 70; i++)
        {
            var product = products[random.Next(products.Count)];
            var maxQty = Math.Min(product.Quantity, 20);

            if (maxQty <= 0) continue;

            var sale = new Sale
            {
                ProductId = product.Id,
                Quantity = random.Next(1, maxQty),
                UnitSalePrice = product.SalePrice * (decimal)(0.9 + random.NextDouble() * 0.2),
                Reason = reasons[random.Next(reasons.Length)],
                SaleDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-random.Next(0, 365)))
            };
            Add(sale);
        }
    }
}
