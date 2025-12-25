using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.SqlServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly WarehouseDbContext _context;

        public ProductRepository(WarehouseDbContext context)
        {
            _context = context;
        }

        public List<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public Product? GetById(Guid id)
        {
            return _context.Products.Find(id);
        }

        public void Add(Product product)
        {
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            var existing = GetById(product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Category = product.Category;
                existing.Manufacturer = product.Manufacturer;
                existing.Model = product.Model;
                existing.Quantity = product.Quantity;
                existing.MinStock = product.MinStock;
                existing.PurchasePrice = product.PurchasePrice;
                existing.SalePrice = product.SalePrice;

                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var product = GetById(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }
    }
}