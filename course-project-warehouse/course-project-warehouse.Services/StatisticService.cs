using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace course_project_warehouse.Services;
using course_project_warehouse.Data.Interfaces;
using course_project_warehouse.Domain.Models;
using course_project_warehouse.Domain.Statistics;


public class StatisticsService
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IShipmentRepository _shipmentRepository;

    public StatisticsService(
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IShipmentRepository shipmentRepository)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _shipmentRepository = shipmentRepository;
    }

    /// <summary>
    /// 1. Отчёт по остаткам — текущие остатки товаров (столбчатая диаграмма)
    /// </summary>
    public List<StockStatisticItem> GetCurrentStock()
    {
        var products = _productRepository.GetAll();

        return products
            .Select(p => new StockStatisticItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Quantity = p.Quantity
            })
            .OrderByDescending(s => s.Quantity)  // Сортировка по убыванию остатка
            .ToList();
    }

    /// <summary>
    /// 2. Отчёт о продажах за период — динамика количества проданных единиц по месяцам (линейная диаграмма)
    /// </summary>
    public List<MonthStatisticItem> GetSalesByMonth(CommonFilter filter)
    {
        var salesFilter = new SalesFilter
        {
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
            // ProductId и Reason можно добавить при необходимости
        };

        var sales = _saleRepository.GetAll(salesFilter);

        return sales
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .Select(g => new MonthStatisticItem
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Value = g.Sum(s => s.Quantity)  // Сумма проданных единиц
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    /// <summary>
    /// 3. Отчёт о прибыли — прибыль по месяцам (выручка от продаж − себестоимость поступлений)
    /// </summary>
    public List<MonthStatisticItem> GetProfitByMonth(CommonFilter filter)
    {
        var salesFilter = new SalesFilter
        {
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };
        var shipmentFilter = new ShipmentFilter
        {
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };

        var sales = _saleRepository.GetAll(salesFilter);
        var shipments = _shipmentRepository.GetAll(shipmentFilter);

        // Выручка по месяцам
        var revenueByMonth = sales
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(s => s.TotalRevenue));

        // Себестоимость по месяцам
        var costByMonth = shipments
            .GroupBy(sh => new { sh.ShipmentDate.Year, sh.ShipmentDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(sh => sh.TotalCost));

        // Объединяем все месяцы
        var allMonths = revenueByMonth.Keys.Union(costByMonth.Keys);

        return allMonths
            .Select(m => new MonthStatisticItem
            {
                Year = m.Year,
                Month = m.Month,
                Value = revenueByMonth.GetValueOrDefault(m, 0m) - costByMonth.GetValueOrDefault(m, 0m)
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    /// <summary>
    /// 4. Отчёт по движению товара — полная история прихода и расхода по конкретному товару
    /// </summary>
    public List<MovementStatisticItem> GetProductMovement(Guid productId, CommonFilter filter)
    {
        var product = _productRepository.GetById(productId);
        if (product == null)
            return new List<MovementStatisticItem>();

        var salesFilter = new SalesFilter
        {
            ProductId = productId,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };
        var sales = _saleRepository.GetAll(salesFilter);

        var shipmentFilter = new ShipmentFilter
        {
            ProductId = productId,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate
        };
        var shipments = _shipmentRepository.GetAll(shipmentFilter);

        var movement = new List<MovementStatisticItem>();

        foreach (var sh in shipments)
        {
            movement.Add(new MovementStatisticItem
            {
                Date = sh.ShipmentDate,
                Type = MovementType.Incoming,
                Quantity = sh.Quantity,
                Amount = -sh.TotalCost
                // Description удалён
            });
        }

        foreach (var s in sales)
        {
            var type = s.Reason == ReasonType.Sale ? MovementType.Sales : MovementType.Writeoff;

            movement.Add(new MovementStatisticItem
            {
                Date = s.SaleDate,
                Type = type,
                Quantity = -s.Quantity,
                Amount = s.TotalRevenue
                // Description удалён
            });
        }

        return movement
            .OrderBy(m => m.Date)
            .ToList();
    }
}