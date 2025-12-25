using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.SqlServer
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly WarehouseDbContext _context;

        public PaymentRepository(WarehouseDbContext context)
        {
            _context = context;
        }

        public List<Payment> GetAll()
        {
            return _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public Payment? GetById(Guid id)
        {
            return _context.Payments.Find(id);
        }

        public void Add(Payment payment)
        {
            payment.Id = Guid.NewGuid();
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);
            _context.SaveChanges();
        }

        public void Update(Payment payment)
        {
            var existing = GetById(payment.Id);
            if (existing != null)
            {
                existing.Amount = payment.Amount;
                existing.PaymentMethod = payment.PaymentMethod;
                existing.PaymentDate = payment.PaymentDate;

                _context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            var payment = GetById(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                _context.SaveChanges();
            }
        }

        public List<Payment> GetByShipmentId(Guid shipmentId)
        {
            return _context.Payments
                .Where(p => p.ShipmentId == shipmentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public decimal GetTotalPaidForShipment(Guid shipmentId)
        {
            return _context.Payments
                .Where(p => p.ShipmentId == shipmentId)
                .Sum(p => p.Amount);
        }

        public List<Payment> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public decimal GetTotalAmountByMethod(string paymentMethod)
        {
            return _context.Payments
                .Where(p => p.PaymentMethod == paymentMethod)
                .Sum(p => p.Amount);
        }
    }
}