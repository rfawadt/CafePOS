using CafePos.Domain.Services;
using Xunit;

namespace CafePos.Tests;

public class PricingTests
{
    [Fact]
    public void CalculatesExclusiveTaxCorrectly()
    {
        var result = OrderPricing.CalculateLine(10m, 1m, 0m, 0.10m, false, Array.Empty<decimal>());
        Assert.Equal(10m, result.Net);
        Assert.Equal(1m, result.Tax);
        Assert.Equal(11m, result.Total);
    }

    [Fact]
    public void CalculatesInclusiveTaxCorrectly()
    {
        var result = OrderPricing.CalculateLine(11m, 1m, 0m, 0.10m, true, Array.Empty<decimal>());
        Assert.Equal(10m, result.Net);
        Assert.Equal(1m, result.Tax);
        Assert.Equal(11m, result.Total);
    }

    [Fact]
    public void IncludesModifierPricing()
    {
        var result = OrderPricing.CalculateLine(4m, 2m, 0m, 0m, false, new[] { 0.5m, 0.5m });
        Assert.Equal(10m, result.Total);
    }

    [Fact]
    public void RoundsAwayFromZero()
    {
        var rounded = OrderPricing.RoundMoney(1.005m);
        Assert.Equal(1.01m, rounded);
    }
}
