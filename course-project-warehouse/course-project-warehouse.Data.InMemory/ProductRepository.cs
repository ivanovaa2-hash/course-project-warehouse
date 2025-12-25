using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace course_project_warehouse.Data.InMemory;
public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public ProductRepository()
    {
        SeedData();
    }

    public List<Product> GetAll() => _products.ToList();

    public Product? GetById(Guid id) => _products.FirstOrDefault(p => p.Id == id);

    public void Add(Product product)
    {
        product.Id = Guid.NewGuid();
        _products.Add(product);
    }

    public void Update(Product product)
    {
        var existing = GetById(product.Id);
        if (existing != null)
        {
            existing.Name = product.Name;
            existing.Category = product.Category;
            existing.Quantity = product.Quantity;
            existing.MinStock = product.MinStock;
            existing.PurchasePrice = product.PurchasePrice;
            existing.SalePrice = product.SalePrice;
        }
    }

    public void Delete(Guid id)
    {
        var product = GetById(id);
        if (product != null) _products.Remove(product);
    }

    private void SeedData()
    {
        var products = new[]
        {
            new Product { Name = "Ноутбук Lenovo", Category = "Электроника", Quantity = 15, MinStock = 5, PurchasePrice = 45000m, SalePrice = 60000m },
            new Product { Name = "Смартфон Samsung", Category = "Электроника", Quantity = 30, MinStock = 10, PurchasePrice = 25000m, SalePrice = 35000m },
            new Product { Name = "Кофе Арабика", Category = "Продукты", Quantity = 100, MinStock = 20, PurchasePrice = 500m, SalePrice = 800m },
            new Product { Name = "Футболка", Category = "Одежда", Quantity = 200, MinStock = 50, PurchasePrice = 300m, SalePrice = 700m },
            new Product { Name = "Книга 'C# в примерах'", Category = "Книги", Quantity = 50, MinStock = 10, PurchasePrice = 800m, SalePrice = 1200m }
        };

        foreach (var p in products) Add(p);
    }
}
