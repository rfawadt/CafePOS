namespace CafePos.Domain.Services;

public static class ShiftCashCalculator
{
    public static ShiftCashResult CalculateExpectedCash(
        decimal openingFloat,
        decimal cashSales,
        decimal payIns,
        decimal payOuts,
        decimal cashDrops,
        decimal cashRefunds)
    {
        var expected = openingFloat + cashSales + payIns - payOuts - cashDrops - cashRefunds;
        return new ShiftCashResult(expected);
    }
}

public readonly record struct ShiftCashResult(decimal ExpectedCash);
