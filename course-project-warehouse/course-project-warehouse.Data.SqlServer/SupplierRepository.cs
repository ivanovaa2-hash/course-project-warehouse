using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.SqlServer
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly WarehouseDbContext _context;

        public SupplierRepository(WarehouseDbContext context)
        {
            _context = context;
        }

        public List<Supplier> GetAll()
        {
            return _context.Suppliers
                .OrderBy(s => s.Name)
                .ToList();
        }

        public Supplier? GetById(Guid id)
        {
            return _context.Suppliers.Find(id);
        }

        public void Add(Supplier supplier)
        {
            supplier.Id = Guid.NewGuid();
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();
        }

        public void Update(Supplier supplier)
        {
            var existing = GetById(supplier.Id);
            if (existing != null)
            {
                existing.Name = supplier.Name;
                existing.Phone = supplier.Phone;
                existing.Email = supplier.Email;

                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var supplier = GetById(id);
            if (supplier != null)
            {
                // Проверяем, нет ли связанных поставок
                var hasShipments = _context.Shipments.Any(s => s.SupplierId == id);

                if (hasShipments)
                {
                    throw new InvalidOperationException(
                        $"Невозможно удалить поставщика '{supplier.Name}'. Существуют связанные поставки.");
                }

                _context.Suppliers.Remove(supplier);
                _context.SaveChanges();
            }
        }

        public Supplier? GetByName(string name)
        {
            return _context.Suppliers
                .FirstOrDefault(s => s.Name.ToLower() == name.ToLower());
        }

        public List<Supplier> Search(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return _context.Suppliers
                .Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.Email != null && s.Email.ToLower().Contains(term)) ||
                    (s.Phone != null && s.Phone.ToLower().Contains(term)))
                .OrderBy(s => s.Name)
                .ToList();
        }

        public int GetShipmentCount(Guid supplierId)
        {
            return _context.Shipments
                .Count(s => s.SupplierId == supplierId);
        }

        public decimal GetTotalSuppliedAmount(Guid supplierId)
        {
            return _context.Shipments
                .Where(s => s.SupplierId == supplierId)
                .Sum(s => s.TotalCost);
        }
    }
}