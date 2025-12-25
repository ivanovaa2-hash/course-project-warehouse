using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace course_project_warehouse.Data.InMemory
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments = new();

        public List<Payment> GetAll()
        {
            return _payments.OrderByDescending(p => p.PaymentDate).ToList();
        }

        public Payment? GetById(Guid id)
        {
            return _payments.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Payment payment)
        {
            payment.Id = Guid.NewGuid();
            payment.PaymentDate = DateTime.Now; // Автоматически устанавливаем текущую дату
            _payments.Add(payment);
        }

        public void Update(Payment payment)
        {
            var existing = GetById(payment.Id);
            if (existing != null)
            {
                existing.Amount = payment.Amount;
                existing.PaymentMethod = payment.PaymentMethod;
                existing.PaymentDate = payment.PaymentDate;
                existing.ShipmentId = payment.ShipmentId;
            }
        }

        public void Delete(Guid id)
        {
            var payment = GetById(id);
            if (payment != null)
            {
                _payments.Remove(payment);
            }
        }

        // Дополнительные методы из SqlServer-реализации (чтобы не терять функционал)
        public List<Payment> GetByShipmentId(Guid shipmentId)
        {
            return _payments
                .Where(p => p.ShipmentId == shipmentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public decimal GetTotalPaidForShipment(Guid shipmentId)
        {
            return _payments
                .Where(p => p.ShipmentId == shipmentId)
                .Sum(p => p.Amount);
        }
    }
}