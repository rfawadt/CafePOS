using CafePos.Domain.Services;
using Xunit;

namespace CafePos.Tests;

public class ShiftCashTests
{
    [Fact]
    public void CalculatesExpectedCash()
    {
        var result = ShiftCashCalculator.CalculateExpectedCash(100m, 50m, 10m, 5m, 20m, 5m);
        Assert.Equal(130m, result.ExpectedCash);
    }
}
