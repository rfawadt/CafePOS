using CafePos.Application.Interfaces;
using CafePos.Application.Models;

namespace CafePos.Application.Services;

public class ReportExportService
{
    private readonly IPosDbContext _db;

    public ReportExportService(IPosDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<OrderExportRow>> GetOrdersAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
    {
        var orders = _db.Orders
            .Where(o => o.CompletedAtLocal != null)
            .Where(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value) >= start && DateOnly.FromDateTime(o.CompletedAtLocal!.Value) <= end)
            .OrderBy(o => o.CompletedAtLocal)
            .Select(o => new OrderExportRow(
                o.ReceiptNumber,
                o.CompletedAtLocal,
                o.Status.ToString(),
                o.Subtotal,
                o.TaxTotal,
                o.Total,
                o.TotalPaid))
            .ToList();

        return Task.FromResult<IReadOnlyList<OrderExportRow>>(orders);
    }

    public Task<IReadOnlyList<OrderLineExportRow>> GetOrderLinesAsync(DateOnly start, DateOnly end, CancellationToken cancellationToken = default)
    {
        var orders = _db.Orders
            .Where(o => o.CompletedAtLocal != null)
            .Where(o => DateOnly.FromDateTime(o.CompletedAtLocal!.Value) >= start && DateOnly.FromDateTime(o.CompletedAtLocal!.Value) <= end)
            .ToList();

        var orderIds = orders.Select(o => o.OrderId).ToList();
        var orderLookup = orders.ToDictionary(o => o.OrderId, o => o);
        var lineRows = _db.OrderLines
            .Where(l => orderIds.Contains(l.OrderId))
            .Select(l => new OrderLineExportRow(
                orderLookup[l.OrderId].ReceiptNumber,
                orderLookup[l.OrderId].CompletedAtLocal,
                l.DescriptionSnapshot,
                l.Qty,
                l.LineTotal,
                l.TaxAmount))
            .ToList();

        return Task.FromResult<IReadOnlyList<OrderLineExportRow>>(lineRows);
    }
}
