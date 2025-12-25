using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.SqlServer
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly WarehouseDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;

        public ShipmentRepository(
            WarehouseDbContext context,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
        }

        public List<Shipment> GetAll(ShipmentFilter filter)
        {
            var query = _context.Shipments.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.ShipmentDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.ShipmentDate <= filter.EndDate.Value);

            if (filter.ProductId.HasValue)
                query = query.Where(x => x.ProductId == filter.ProductId.Value);

            if (filter.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == filter.SupplierId.Value);

            return query
                .OrderByDescending(s => s.ShipmentDate)
                .ToList();
        }

        public void Add(Shipment shipment)
        {
            shipment.Id = Guid.NewGuid();

            // Обновляем остаток товара
            var product = _productRepository.GetById(shipment.ProductId);
            if (product != null)
            {
                product.Quantity += shipment.Quantity;
                _productRepository.Update(product);
            }
            else
            {
                throw new InvalidOperationException($"Товар с ID {shipment.ProductId} не найден");
            }

            _context.Shipments.Add(shipment);
            _context.SaveChanges();
        }

        public Shipment? GetById(Guid id)
        {
            return _context.Shipments.Find(id);
        }

        public List<Shipment> GetByProductId(Guid productId)
        {
            return _context.Shipments
                .Where(s => s.ProductId == productId)
                .OrderByDescending(s => s.ShipmentDate)
                .ToList();
        }

        public List<Shipment> GetBySupplierId(Guid supplierId)
        {
            return _context.Shipments
                .Where(s => s.SupplierId == supplierId)
                .OrderByDescending(s => s.ShipmentDate)
                .ToList();
        }

        public decimal GetTotalCost(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var query = _context.Shipments.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.ShipmentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.ShipmentDate <= endDate.Value);

            return query.Sum(s => s.TotalCost);
        }

        public void Delete(Guid id)
        {
            var shipment = GetById(id);
            if (shipment != null)
            {
                // Возвращаем товар на склад при удалении поставки
                var product = _productRepository.GetById(shipment.ProductId);
                if (product != null)
                {
                    product.Quantity -= shipment.Quantity;
                    _productRepository.Update(product);
                }

                _context.Shipments.Remove(shipment);
                _context.SaveChanges();
            }
        }
    }
}