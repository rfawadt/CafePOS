namespace CafePos.Domain.Services;

public static class OrderPricing
{
    public const int MoneyScale = 2;

    public static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, MoneyScale, MidpointRounding.AwayFromZero);
    }

    public static LinePricingResult CalculateLine(decimal unitPrice, decimal qty, decimal discount, decimal taxRate, bool taxInclusive, IEnumerable<decimal> modifierDeltas)
    {
        var modifiersTotal = modifierDeltas.Sum();
        var effectiveUnitPrice = unitPrice + modifiersTotal;
        var lineNet = (effectiveUnitPrice * qty) - discount;
        lineNet = RoundMoney(lineNet);

        if (taxRate <= 0m)
        {
            return new LinePricingResult(lineNet, 0m, lineNet);
        }

        if (taxInclusive)
        {
            var netOfTax = RoundMoney(lineNet / (1m + taxRate));
            var tax = RoundMoney(lineNet - netOfTax);
            return new LinePricingResult(netOfTax, tax, lineNet);
        }

        var taxAmount = RoundMoney(lineNet * taxRate);
        var lineTotal = RoundMoney(lineNet + taxAmount);
        return new LinePricingResult(lineNet, taxAmount, lineTotal);
    }

    public static OrderTotalsResult CalculateOrderTotals(IEnumerable<LineTotals> lines)
    {
        var subtotal = RoundMoney(lines.Sum(l => l.Net));
        var taxTotal = RoundMoney(lines.Sum(l => l.Tax));
        var total = RoundMoney(lines.Sum(l => l.Total));
        return new OrderTotalsResult(subtotal, taxTotal, total);
    }
}

public readonly record struct LinePricingResult(decimal Net, decimal Tax, decimal Total);

public readonly record struct LineTotals(decimal Net, decimal Tax, decimal Total);

public readonly record struct OrderTotalsResult(decimal Subtotal, decimal TaxTotal, decimal Total);
