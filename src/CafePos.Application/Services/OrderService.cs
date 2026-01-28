using CafePos.Application.Interfaces;
using CafePos.Application.Models;
using CafePos.Domain.Entities;
using CafePos.Domain.Enums;
using CafePos.Domain.Services;

namespace CafePos.Application.Services;

public class OrderService
{
    private readonly IPosDbContext _db;
    private readonly IReceiptNumberService _receiptNumbers;
    private readonly IPrinterService _printer;
    private readonly IClock _clock;

    public OrderService(IPosDbContext db, IReceiptNumberService receiptNumbers, IPrinterService printer, IClock clock)
    {
        _db = db;
        _receiptNumbers = receiptNumbers;
        _printer = printer;
        _clock = clock;
    }

    public async Task<Order> StartOrderAsync(Guid storeId, Guid terminalId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            StoreId = storeId,
            TerminalId = terminalId,
            CreatedAtLocal = _clock.LocalNow,
            CreatedByUserId = userId,
            Status = OrderStatus.Open
        };
        await _db.AddAsync(order, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task HoldOrderAsync(Guid orderId, string? heldName, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        order.Status = OrderStatus.Held;
        order.HeldName = heldName;
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecallOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        order.Status = OrderStatus.Open;
        order.HeldName = null;
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderLine> AddItemToOrderAsync(Guid orderId, Guid itemPriceId, IReadOnlyList<Guid> modifierOptionIds, string? lineNote, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }
        if (order.Status != OrderStatus.Open && order.Status != OrderStatus.Held)
        {
            throw new InvalidOperationException("Order is not editable.");
        }

        var price = _db.MenuItemPrices.FirstOrDefault(p => p.ItemPriceId == itemPriceId && p.IsActive);
        if (price == null)
        {
            throw new InvalidOperationException("Item price not found.");
        }

        var item = _db.MenuItems.FirstOrDefault(i => i.ItemId == price.ItemId && i.IsActive);
        if (item == null)
        {
            throw new InvalidOperationException("Menu item not found.");
        }

        var taxCategory = price.TaxCategoryId == null
            ? null
            : _db.TaxCategories.FirstOrDefault(t => t.TaxCategoryId == price.TaxCategoryId && t.IsActive);

        var modifierOptions = modifierOptionIds.Count == 0
            ? new List<ModifierOption>()
            : _db.ModifierOptions.Where(m => modifierOptionIds.Contains(m.ModifierOptionId) && m.IsActive).ToList();

        var modifierGroups = modifierOptions.Count == 0
            ? new List<ModifierGroup>()
            : _db.ModifierGroups.Where(g => modifierOptions.Select(m => m.ModifierGroupId).Contains(g.ModifierGroupId)).ToList();

        var line = new OrderLine
        {
            OrderLineId = Guid.NewGuid(),
            OrderId = order.OrderId,
            ItemId = item.ItemId,
            ItemPriceId = price.ItemPriceId,
            DescriptionSnapshot = BuildItemDescription(item.Name, price.Label),
            UnitPriceSnapshot = price.Price,
            Qty = 1m,
            LineDiscount = 0m,
            TaxRateSnapshot = taxCategory?.Rate ?? 0m,
            IsTaxInclusiveSnapshot = taxCategory?.IsInclusive ?? false,
            LineNote = lineNote
        };

        foreach (var option in modifierOptions)
        {
            var groupName = modifierGroups.FirstOrDefault(g => g.ModifierGroupId == option.ModifierGroupId)?.Name ?? string.Empty;
            line.Modifiers.Add(new OrderLineModifier
            {
                LineModifierId = Guid.NewGuid(),
                OrderLineId = line.OrderLineId,
                ModifierGroupNameSnapshot = groupName,
                ModifierOptionNameSnapshot = option.Name,
                PriceDeltaSnapshot = option.PriceDelta
            });
        }

        ApplyLinePricing(line);
        order.Lines = _db.OrderLines.Where(l => l.OrderId == order.OrderId).ToList();
        order.Lines.Add(line);
        ApplyOrderTotals(order);

        await _db.AddAsync(line, cancellationToken);
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
        return line;
    }

    public async Task UpdateLineQtyAsync(Guid orderLineId, decimal qty, CancellationToken cancellationToken = default)
    {
        if (qty <= 0m)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        var line = _db.OrderLines.FirstOrDefault(l => l.OrderLineId == orderLineId);
        if (line == null)
        {
            throw new InvalidOperationException("Line not found.");
        }

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == line.OrderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        line.Qty = qty;
        line.Modifiers = _db.OrderLineModifiers.Where(m => m.OrderLineId == line.OrderLineId).ToList();
        ApplyLinePricing(line);
        order.Lines = _db.OrderLines.Where(l => l.OrderId == order.OrderId).ToList();
        ApplyOrderTotals(order);

        _db.Update(line);
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLineAsync(Guid orderLineId, CancellationToken cancellationToken = default)
    {
        var line = _db.OrderLines.FirstOrDefault(l => l.OrderLineId == orderLineId);
        if (line == null)
        {
            return;
        }

        var order = _db.Orders.FirstOrDefault(o => o.OrderId == line.OrderId);
        if (order == null)
        {
            return;
        }

        var modifiers = _db.OrderLineModifiers.Where(m => m.OrderLineId == line.OrderLineId).ToList();
        foreach (var modifier in modifiers)
        {
            _db.Remove(modifier);
        }

        _db.Remove(line);
        order.Lines = _db.OrderLines.Where(l => l.OrderId == order.OrderId && l.OrderLineId != line.OrderLineId).ToList();
        ApplyOrderTotals(order);
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLineNoteAsync(Guid orderLineId, string? note, CancellationToken cancellationToken = default)
    {
        var line = _db.OrderLines.FirstOrDefault(l => l.OrderLineId == orderLineId);
        if (line == null)
        {
            throw new InvalidOperationException("Line not found.");
        }

        line.LineNote = note;
        _db.Update(line);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteOrderAsync(Guid orderId, Guid terminalId, Guid userId, IEnumerable<PaymentInput> payments, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }
        if (order.Status != OrderStatus.Open && order.Status != OrderStatus.Held)
        {
            throw new InvalidOperationException("Order cannot be completed.");
        }

        var shift = _db.Shifts.FirstOrDefault(s => s.TerminalId == terminalId && s.Status == ShiftStatus.Open);
        if (shift == null)
        {
            throw new InvalidOperationException("No open shift for this terminal.");
        }

        var lineList = _db.OrderLines.Where(l => l.OrderId == orderId).ToList();
        order.Lines = lineList;
        ApplyOrderTotals(order);

        var totalPaid = payments.Sum(p => p.Amount);
        if (totalPaid < order.Total)
        {
            throw new InvalidOperationException("Insufficient payment.");
        }

        await using var transaction = await _db.BeginTransactionAsync(cancellationToken);
        try
        {
            if (string.IsNullOrWhiteSpace(order.ReceiptNumber))
            {
                var receiptNumber = await _receiptNumbers.NextReceiptNumberAsync(order.StoreId, order.TerminalId, _clock.LocalDate, cancellationToken);
                order.ReceiptNumber = receiptNumber;
            }

            var paymentEntities = payments.Select(p => new Payment
            {
                PaymentId = Guid.NewGuid(),
                OrderId = order.OrderId,
                PaymentMethodId = p.PaymentMethodId,
                Amount = p.Amount,
                Reference = p.Reference,
                CapturedAtLocal = _clock.LocalNow,
                IsRefund = false
            }).ToList();

            await _db.AddRangeAsync(paymentEntities, cancellationToken);

            order.TotalPaid = totalPaid;
            order.ChangeDue = Math.Max(0m, totalPaid - order.Total);
            order.Status = OrderStatus.Completed;
            order.CompletedAtLocal = _clock.LocalNow;
            order.CompletedByUserId = userId;
            order.ShiftId = shift.ShiftId;

            _db.Update(order);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        await PrintReceiptAsync(orderId, cancellationToken);
    }

    public async Task RefundOrderAsync(Guid orderId, Guid userId, IEnumerable<PaymentInput> refundPayments, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }
        if (order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Only completed orders can be refunded.");
        }

        await using var transaction = await _db.BeginTransactionAsync(cancellationToken);
        try
        {
            var paymentEntities = refundPayments.Select(p => new Payment
            {
                PaymentId = Guid.NewGuid(),
                OrderId = order.OrderId,
                PaymentMethodId = p.PaymentMethodId,
                Amount = p.Amount,
                Reference = p.Reference,
                CapturedAtLocal = _clock.LocalNow,
                IsRefund = true
            }).ToList();

            await _db.AddRangeAsync(paymentEntities, cancellationToken);

            order.Status = OrderStatus.Refunded;
            order.CompletedByUserId = userId;

            _db.Update(order);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task VoidOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }
        if (order.Status != OrderStatus.Open && order.Status != OrderStatus.Held)
        {
            throw new InvalidOperationException("Only open/held orders can be voided.");
        }

        order.Status = OrderStatus.Voided;
        order.CompletedByUserId = userId;
        order.CompletedAtLocal = _clock.LocalNow;
        _db.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string BuildItemDescription(string name, string? label)
    {
        return string.IsNullOrWhiteSpace(label) ? name : $"{name} - {label}";
    }

    private static void ApplyLinePricing(OrderLine line)
    {
        var modifiersTotal = line.Modifiers.Select(m => m.PriceDeltaSnapshot);
        var result = OrderPricing.CalculateLine(
            line.UnitPriceSnapshot,
            line.Qty,
            line.LineDiscount,
            line.TaxRateSnapshot,
            line.IsTaxInclusiveSnapshot,
            modifiersTotal);

        line.TaxAmount = result.Tax;
        line.LineTotal = result.Total;
    }

    private static void ApplyOrderTotals(Order order)
    {
        var lines = order.Lines.Select(l => new LineTotals(l.LineTotal - l.TaxAmount, l.TaxAmount, l.LineTotal));
        var totals = OrderPricing.CalculateOrderTotals(lines);
        order.Subtotal = totals.Subtotal;
        order.TaxTotal = totals.TaxTotal;
        order.Total = totals.Total;
        order.DiscountTotal = order.Lines.Sum(l => l.LineDiscount);
    }

    private async Task PrintReceiptAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order == null)
        {
            return;
        }

        var store = _db.Stores.FirstOrDefault(s => s.StoreId == order.StoreId);
        var terminal = _db.Terminals.FirstOrDefault(t => t.TerminalId == order.TerminalId);
        var user = _db.Users.FirstOrDefault(u => u.UserId == order.CreatedByUserId);
        var lines = _db.OrderLines.Where(l => l.OrderId == order.OrderId).ToList();
        var modifiers = _db.OrderLineModifiers.Where(m => lines.Select(l => l.OrderLineId).Contains(m.OrderLineId)).ToList();
        var payments = _db.Payments.Where(p => p.OrderId == order.OrderId).ToList();
        var paymentMethods = _db.PaymentMethods.Where(pm => payments.Select(p => p.PaymentMethodId).Contains(pm.PaymentMethodId)).ToList();

        var receipt = new ReceiptData
        {
            StoreName = store?.Name ?? "",
            StoreAddress = store == null ? string.Empty : $"{store.AddressLine1}, {store.City}",
            StorePhone = store?.Phone ?? string.Empty,
            ReceiptNumber = order.ReceiptNumber,
            PrintedAtLocal = _clock.LocalNow,
            TerminalName = terminal?.Name ?? string.Empty,
            CashierName = user?.DisplayName ?? string.Empty,
            Subtotal = order.Subtotal,
            TaxTotal = order.TaxTotal,
            Total = order.Total,
            ChangeDue = order.ChangeDue,
            Footer = "Thank you!"
        };

        foreach (var line in lines)
        {
            var lineModifiers = modifiers.Where(m => m.OrderLineId == line.OrderLineId)
                .Select(m => new ReceiptModifier { Name = $"{m.ModifierGroupNameSnapshot}: {m.ModifierOptionNameSnapshot}", PriceDelta = m.PriceDeltaSnapshot })
                .ToList();

            receipt.Lines.Add(new ReceiptLine
            {
                Description = line.DescriptionSnapshot,
                Qty = line.Qty,
                LineTotal = line.LineTotal,
                Modifiers = lineModifiers,
                LineNote = line.LineNote
            });
        }

        foreach (var payment in payments)
        {
            var method = paymentMethods.FirstOrDefault(pm => pm.PaymentMethodId == payment.PaymentMethodId);
            receipt.Payments.Add(new ReceiptPayment
            {
                Method = method?.Name ?? "Payment",
                Amount = payment.Amount,
                IsRefund = payment.IsRefund
            });
        }

        await _printer.PrintReceiptAsync(receipt, cancellationToken);
    }
}
