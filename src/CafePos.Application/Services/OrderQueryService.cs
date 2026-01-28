using CafePos.Application.Interfaces;
using CafePos.Application.Models;
using CafePos.Domain.Enums;

namespace CafePos.Application.Services;

public class OrderQueryService
{
    private readonly IPosDbContext _db;

    public OrderQueryService(IPosDbContext db)
    {
        _db = db;
    }

    public Task<OrderDetailDto?> GetOrderDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            return Task.FromResult<OrderDetailDto?>(null);
        }

        var lines = _db.OrderLines.Where(l => l.OrderId == orderId).ToList();
        var dto = new OrderDetailDto
        {
            OrderId = order.OrderId,
            Subtotal = order.Subtotal,
            TaxTotal = order.TaxTotal,
            Total = order.Total,
            Lines = lines.Select(l => new OrderLineDto
            {
                OrderLineId = l.OrderLineId,
                Description = l.DescriptionSnapshot,
                Qty = l.Qty,
                LineTotal = l.LineTotal,
                LineNote = l.LineNote
            }).ToList()
        };

        return Task.FromResult<OrderDetailDto?>(dto);
    }

    public Task<IReadOnlyList<HeldOrderDto>> GetHeldOrdersAsync(Guid terminalId, CancellationToken cancellationToken = default)
    {
        var heldOrders = _db.Orders
            .Where(o => o.TerminalId == terminalId && o.Status == OrderStatus.Held)
            .OrderBy(o => o.CreatedAtLocal)
            .Select(o => new HeldOrderDto
            {
                OrderId = o.OrderId,
                HeldName = o.HeldName,
                CreatedAtLocal = o.CreatedAtLocal,
                Total = o.Total
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<HeldOrderDto>>(heldOrders);
    }
}
