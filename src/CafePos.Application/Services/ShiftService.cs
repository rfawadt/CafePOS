using CafePos.Application.Interfaces;
using CafePos.Application.Models;
using CafePos.Domain.Entities;
using CafePos.Domain.Enums;
using CafePos.Domain.Services;

namespace CafePos.Application.Services;

public class ShiftService
{
    private readonly IPosDbContext _db;
    private readonly IClock _clock;

    public ShiftService(IPosDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public Task<Shift?> GetOpenShiftAsync(Guid terminalId, CancellationToken cancellationToken = default)
    {
        var shift = _db.Shifts.FirstOrDefault(s => s.TerminalId == terminalId && s.Status == ShiftStatus.Open);
        return Task.FromResult(shift);
    }

    public async Task<Shift> OpenShiftAsync(Guid storeId, Guid terminalId, Guid userId, decimal openingFloat, CancellationToken cancellationToken = default)
    {
        var existing = _db.Shifts.FirstOrDefault(s => s.TerminalId == terminalId && s.Status == ShiftStatus.Open);
        if (existing != null)
        {
            throw new InvalidOperationException("Shift already open for this terminal.");
        }

        var shift = new Shift
        {
            ShiftId = Guid.NewGuid(),
            StoreId = storeId,
            TerminalId = terminalId,
            OpenedByUserId = userId,
            OpenedAtLocal = _clock.LocalNow,
            OpeningFloat = openingFloat,
            Status = ShiftStatus.Open
        };

        await _db.AddAsync(shift, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return shift;
    }

    public async Task<ShiftSummaryDto> CloseShiftAsync(Guid shiftId, Guid userId, decimal countedCash, CancellationToken cancellationToken = default)
    {
        using var transaction = await _db.BeginTransactionAsync(cancellationToken);
        try
        {
            var shift = _db.Shifts.FirstOrDefault(s => s.ShiftId == shiftId);
            if (shift == null)
            {
                throw new InvalidOperationException("Shift not found.");
            }
            if (shift.Status != ShiftStatus.Open)
            {
                throw new InvalidOperationException("Shift is already closed.");
            }

            var summary = BuildShiftSummary(shift);
            var expected = summary.ExpectedCash;
            var variance = countedCash - expected;

            shift.CountedCash = countedCash;
            shift.ExpectedCash = expected;
            shift.Variance = variance;
            shift.ClosedAtLocal = _clock.LocalNow;
            shift.ClosedByUserId = userId;
            shift.Status = ShiftStatus.Closed;

            _db.Update(shift);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            summary.CountedCash = countedCash;
            summary.Variance = variance;
            summary.ClosedAtLocal = shift.ClosedAtLocal;
            return summary;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public Task<ShiftSummaryDto> GetShiftSummaryAsync(Guid shiftId, CancellationToken cancellationToken = default)
    {
        var shift = _db.Shifts.FirstOrDefault(s => s.ShiftId == shiftId);
        if (shift == null)
        {
            throw new InvalidOperationException("Shift not found.");
        }

        return Task.FromResult(BuildShiftSummary(shift));
    }

    private ShiftSummaryDto BuildShiftSummary(Shift shift)
    {
        var orders = _db.Orders.Where(o => o.ShiftId == shift.ShiftId).ToList();
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Refunded || o.Status == OrderStatus.PartiallyRefunded).ToList();
        var voidedOrders = orders.Where(o => o.Status == OrderStatus.Voided).ToList();
        var payments = _db.Payments.Where(p => orders.Select(o => o.OrderId).Contains(p.OrderId)).ToList();
        var paymentMethods = _db.PaymentMethods.Where(pm => payments.Select(p => p.PaymentMethodId).Contains(pm.PaymentMethodId)).ToList();

        var grossSales = completedOrders.Sum(o => o.Total);
        var discountsTotal = completedOrders.Sum(o => o.DiscountTotal);
        var taxTotal = completedOrders.Sum(o => o.TaxTotal);
        var refundsTotal = payments.Where(p => p.IsRefund).Sum(p => p.Amount);
        var netSales = grossSales - refundsTotal;
        var ordersCount = completedOrders.Count;
        var averageTicket = ordersCount == 0 ? 0m : grossSales / ordersCount;
        var voidsTotal = voidedOrders.Sum(o => o.Total);

        var paymentBreakdown = paymentMethods.Select(pm =>
        {
            var paid = payments.Where(p => p.PaymentMethodId == pm.PaymentMethodId && !p.IsRefund).Sum(p => p.Amount);
            var refunded = payments.Where(p => p.PaymentMethodId == pm.PaymentMethodId && p.IsRefund).Sum(p => p.Amount);
            return new PaymentBreakdownItem(pm.Name, paid, refunded);
        }).ToList();

        var cashMethodIds = paymentMethods.Where(pm => pm.Type == PaymentMethodType.Cash).Select(pm => pm.PaymentMethodId).ToList();
        var cashSales = payments.Where(p => cashMethodIds.Contains(p.PaymentMethodId) && !p.IsRefund).Sum(p => p.Amount);
        var cashRefunds = payments.Where(p => cashMethodIds.Contains(p.PaymentMethodId) && p.IsRefund).Sum(p => p.Amount);

        var events = _db.CashDrawerEvents.Where(e => e.ShiftId == shift.ShiftId).ToList();
        var payIns = events.Where(e => e.Type == CashDrawerEventType.PayIn).Sum(e => e.Amount);
        var payOuts = events.Where(e => e.Type == CashDrawerEventType.PayOut).Sum(e => e.Amount);
        var drops = events.Where(e => e.Type == CashDrawerEventType.CashDrop).Sum(e => e.Amount);

        var expectedCash = ShiftCashCalculator.CalculateExpectedCash(shift.OpeningFloat, cashSales, payIns, payOuts, drops, cashRefunds).ExpectedCash;

        var lines = _db.OrderLines.Where(l => completedOrders.Select(o => o.OrderId).Contains(l.OrderId)).ToList();
        var topItems = lines
            .GroupBy(l => l.DescriptionSnapshot)
            .Select(g => new TopItemDto(g.Key, g.Sum(x => x.Qty), g.Sum(x => x.LineTotal)))
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToList();

        return new ShiftSummaryDto
        {
            ShiftId = shift.ShiftId,
            OpenedAtLocal = shift.OpenedAtLocal,
            ClosedAtLocal = shift.ClosedAtLocal,
            OpeningFloat = shift.OpeningFloat,
            GrossSales = grossSales,
            NetSales = netSales,
            TaxTotal = taxTotal,
            DiscountsTotal = discountsTotal,
            RefundsTotal = refundsTotal,
            VoidsTotal = voidsTotal,
            OrdersCount = ordersCount,
            AverageTicket = averageTicket,
            ExpectedCash = expectedCash,
            CountedCash = shift.CountedCash,
            Variance = shift.Variance,
            PaymentBreakdown = paymentBreakdown,
            TopItems = topItems
        };
    }
}
