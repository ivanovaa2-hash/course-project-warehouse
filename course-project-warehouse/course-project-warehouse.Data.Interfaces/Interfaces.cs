using course_project_warehouse.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using course_project_warehouse.Data.Interfaces;

namespace course_project_warehouse.Data.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        Product? GetById(Guid id);
        void Add(Product product);
        void Update(Product product);
        void Delete(Guid id);
    }

    public interface ISupplierRepository
    {
        List<Supplier> GetAll();
        Supplier? GetById(Guid id);
        void Add(Supplier supplier);
    }

    public interface ISaleRepository
    {
        List<Sale> GetAll(SalesFilter filter);
        void Add(Sale sale);
    }

    public interface IShipmentRepository
    {
        List<Shipment> GetAll(ShipmentFilter filter);
        void Add(Shipment shipment);
    }

    public interface IPaymentRepository
    {
        List<Payment> GetAll();
        Payment? GetById(Guid id);
        void Add(Payment payment);
        void Update(Payment payment);
        void Delete(Guid id);

        List<Payment> GetByShipmentId(Guid shipmentId);
        decimal GetTotalPaidForShipment(Guid shipmentId);
    }
}
