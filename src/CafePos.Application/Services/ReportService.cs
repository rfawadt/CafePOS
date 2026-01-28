using CafePos.Application.Interfaces;
using CafePos.Application.Models;
using CafePos.Domain.Enums;

namespace CafePos.Application.Services;

public class ReportService
{
    private readonly IPosDbContext _db;

    public ReportService(IPosDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<DailySummaryDto>> GetDailySummaryAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
    {
        var orders = _db.Orders
            .Where(o => (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Refunded || o.Status == OrderStatus.PartiallyRefunded) && o.CompletedAtLocal != null)
            .ToList();

        var voidedOrders = _db.Orders
            .Where(o => o.Status == OrderStatus.Voided && o.CompletedAtLocal != null)
            .ToList();

        var payments = _db.Payments.Where(p => orders.Select(o => o.OrderId).Contains(p.OrderId)).ToList();
        var paymentMethods = _db.PaymentMethods.Where(pm => payments.Select(p => p.PaymentMethodId).Contains(pm.PaymentMethodId)).ToList();

        var lines = _db.OrderLines.Where(l => orders.Select(o => o.OrderId).Contains(l.OrderId)).ToList();
        var items = _db.MenuItems.Where(i => lines.Select(l => l.ItemId).Contains(i.ItemId)).ToList();
        var categories = _db.MenuCategories.Where(c => items.Select(i => i.CategoryId).Contains(c.CategoryId)).ToList();

        var filtered = orders
            .Where(o => o.CompletedAtLocal != null)
            .Where(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value) >= start && DateOnly.FromDateTime(o.CompletedAtLocal!.Value) <= end)
            .ToList();

        var filteredVoids = voidedOrders
            .Where(o => o.CompletedAtLocal != null)
            .Where(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value) >= start && DateOnly.FromDateTime(o.CompletedAtLocal!.Value) <= end)
            .ToList();

        var summaries = filtered
            .GroupBy(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value))
            .Select(group => BuildDailySummary(group.Key, group.ToList(), filteredVoids, lines, payments, paymentMethods, items, categories))
            .OrderBy(s => s.Date)
            .ToList();

        return Task.FromResult<IReadOnlyList<DailySummaryDto>>(summaries);
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var dailySummaries = await GetDailySummaryAsync(start, end, cancellationToken);
        var monthOrders = dailySummaries.Sum(s => s.OrdersCount);
        var gross = dailySummaries.Sum(s => s.GrossSales);
        var net = dailySummaries.Sum(s => s.NetSales);
        var tax = dailySummaries.Sum(s => s.TaxTotal);
        var discounts = dailySummaries.Sum(s => s.DiscountsTotal);
        var refunds = dailySummaries.Sum(s => s.RefundsTotal);
        var voids = dailySummaries.Sum(s => s.VoidsTotal);
        var averageTicket = monthOrders == 0 ? 0m : gross / monthOrders;

        var paymentBreakdown = dailySummaries
            .SelectMany(s => s.PaymentBreakdown)
            .GroupBy(p => p.Method)
            .Select(g => new PaymentBreakdownItem(g.Key, g.Sum(x => x.Amount), g.Sum(x => x.RefundAmount)))
            .ToList();

        var topItems = dailySummaries
            .SelectMany(s => s.TopItems)
            .GroupBy(t => t.Description)
            .Select(g => new TopItemDto(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Revenue)))
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToList();

        var topCategories = dailySummaries
            .SelectMany(s => s.TopCategories)
            .GroupBy(t => t.Description)
            .Select(g => new TopItemDto(g.Key, g.Sum(x => x.Quantity), g.Sum(x => x.Revenue)))
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToList();

        return new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            DailyTotals = dailySummaries.ToList(),
            GrossSales = gross,
            NetSales = net,
            TaxTotal = tax,
            DiscountsTotal = discounts,
            RefundsTotal = refunds,
            VoidsTotal = voids,
            OrdersCount = monthOrders,
            AverageTicket = averageTicket,
            PaymentBreakdown = paymentBreakdown,
            TopItems = topItems,
            TopCategories = topCategories
        };
    }

    private static DailySummaryDto BuildDailySummary(
        DateOnly date,
        List<CafePos.Domain.Entities.Order> orders,
        List<CafePos.Domain.Entities.Order> voidedOrders,
        List<CafePos.Domain.Entities.OrderLine> lines,
        List<CafePos.Domain.Entities.Payment> payments,
        List<CafePos.Domain.Entities.PaymentMethod> paymentMethods,
        List<CafePos.Domain.Entities.MenuItem> items,
        List<CafePos.Domain.Entities.MenuCategory> categories)
    {
        var orderIds = orders.Select(o => o.OrderId).ToList();
        var dayPayments = payments.Where(p => orderIds.Contains(p.OrderId)).ToList();
        var grossSales = orders.Sum(o => o.Total);
        var discountsTotal = orders.Sum(o => o.DiscountTotal);
        var taxTotal = orders.Sum(o => o.TaxTotal);
        var refundsTotal = dayPayments.Where(p => p.IsRefund).Sum(p => p.Amount);
        var netSales = grossSales - refundsTotal;
        var ordersCount = orders.Count;
        var averageTicket = ordersCount == 0 ? 0m : grossSales / ordersCount;
        var voidsTotal = voidedOrders.Where(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value) == date).Sum(o => o.Total);

        var paymentBreakdown = paymentMethods.Select(pm =>
        {
            var paid = dayPayments.Where(p => p.PaymentMethodId == pm.PaymentMethodId && !p.IsRefund).Sum(p => p.Amount);
            var refunded = dayPayments.Where(p => p.PaymentMethodId == pm.PaymentMethodId && p.IsRefund).Sum(p => p.Amount);
            return new PaymentBreakdownItem(pm.Name, paid, refunded);
        }).ToList();

        var dayLines = lines.Where(l => orderIds.Contains(l.OrderId)).ToList();
        var topItems = dayLines
            .GroupBy(l => l.DescriptionSnapshot)
            .Select(g => new TopItemDto(g.Key, g.Sum(x => x.Qty), g.Sum(x => x.LineTotal)))
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToList();

        var topCategories = dayLines
            .GroupBy(l =>
            {
                var item = items.FirstOrDefault(i => i.ItemId == l.ItemId);
                var category = item == null ? null : categories.FirstOrDefault(c => c.CategoryId == item.CategoryId);
                return category?.Name ?? "Unknown";
            })
            .Select(g => new TopItemDto(g.Key, g.Sum(x => x.Qty), g.Sum(x => x.LineTotal)))
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToList();

        return new DailySummaryDto
        {
            Date = date,
            GrossSales = grossSales,
            NetSales = netSales,
            TaxTotal = taxTotal,
            DiscountsTotal = discountsTotal,
            RefundsTotal = refundsTotal,
            VoidsTotal = voidsTotal,
            OrdersCount = ordersCount,
            AverageTicket = averageTicket,
            PaymentBreakdown = paymentBreakdown,
            TopItems = topItems,
            TopCategories = topCategories
        };
    }
}
