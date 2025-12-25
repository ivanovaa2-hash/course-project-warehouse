using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.SqlServer
{
    public class SaleRepository : ISaleRepository
    {
        private readonly WarehouseDbContext _context;
        private readonly IProductRepository _productRepository;

        public SaleRepository(WarehouseDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }

        public List<Sale> GetAll(SalesFilter filter)
        {
            var query = _context.Sales.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.SaleDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.SaleDate <= filter.EndDate.Value);

            if (filter.ProductId.HasValue)
                query = query.Where(x => x.ProductId == filter.ProductId.Value);

            if (filter.Reason.HasValue)
                query = query.Where(x => x.Reason == filter.Reason.Value);

            return query.ToList();
        }

        public void Add(Sale sale)
        {
            sale.Id = Guid.NewGuid();

            // Проверяем и обновляем остаток товара
            var product = _productRepository.GetById(sale.ProductId);
            if (product != null && product.Quantity >= sale.Quantity)
            {
                product.Quantity -= sale.Quantity;
                _productRepository.Update(product);
            }
            else
            {
                throw new InvalidOperationException($"Недостаточный остаток товара. Доступно: {product?.Quantity ?? 0}, требуется: {sale.Quantity}");
            }

            _context.Sales.Add(sale);
            _context.SaveChanges();
        }

        public List<Sale> GetByProductId(Guid productId)
        {
            return _context.Sales
                .Where(s => s.ProductId == productId)
                .ToList();
        }

        public decimal GetTotalRevenue(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _context.Sales.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate <= endDate.Value);

            return query.Sum(s => s.TotalRevenue);
        }
    }
}