namespace CafePos.Application.Models;

public record PaymentBreakdownItem(string Method, decimal Amount, decimal RefundAmount);

public record TopItemDto(string Description, decimal Quantity, decimal Revenue);

public class ShiftSummaryDto
{
    public Guid ShiftId { get; set; }
    public DateTime OpenedAtLocal { get; set; }
    public DateTime? ClosedAtLocal { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal GrossSales { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountsTotal { get; set; }
    public decimal RefundsTotal { get; set; }
    public decimal VoidsTotal { get; set; }
    public int OrdersCount { get; set; }
    public decimal AverageTicket { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? CountedCash { get; set; }
    public decimal? Variance { get; set; }
    public IList<PaymentBreakdownItem> PaymentBreakdown { get; set; } = new List<PaymentBreakdownItem>();
    public IList<TopItemDto> TopItems { get; set; } = new List<TopItemDto>();
}

public class DailySummaryDto
{
    public DateOnly Date { get; set; }
    public decimal GrossSales { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountsTotal { get; set; }
    public decimal RefundsTotal { get; set; }
    public decimal VoidsTotal { get; set; }
    public int OrdersCount { get; set; }
    public decimal AverageTicket { get; set; }
    public IList<PaymentBreakdownItem> PaymentBreakdown { get; set; } = new List<PaymentBreakdownItem>();
    public IList<TopItemDto> TopItems { get; set; } = new List<TopItemDto>();
    public IList<TopItemDto> TopCategories { get; set; } = new List<TopItemDto>();
}

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public IList<DailySummaryDto> DailyTotals { get; set; } = new List<DailySummaryDto>();
    public decimal GrossSales { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountsTotal { get; set; }
    public decimal RefundsTotal { get; set; }
    public decimal VoidsTotal { get; set; }
    public int OrdersCount { get; set; }
    public decimal AverageTicket { get; set; }
    public IList<PaymentBreakdownItem> PaymentBreakdown { get; set; } = new List<PaymentBreakdownItem>();
    public IList<TopItemDto> TopItems { get; set; } = new List<TopItemDto>();
    public IList<TopItemDto> TopCategories { get; set; } = new List<TopItemDto>();
}
