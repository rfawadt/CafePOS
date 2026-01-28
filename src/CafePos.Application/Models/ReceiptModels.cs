namespace CafePos.Application.Models;

public class ReceiptData
{
    public string StoreName { get; set; } = string.Empty;
    public string StoreAddress { get; set; } = string.Empty;
    public string StorePhone { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PrintedAtLocal { get; set; }
    public string TerminalName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public IList<ReceiptLine> Lines { get; set; } = new List<ReceiptLine>();
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public IList<ReceiptPayment> Payments { get; set; } = new List<ReceiptPayment>();
    public decimal ChangeDue { get; set; }
    public string Footer { get; set; } = string.Empty;
}

public class ReceiptLine
{
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal LineTotal { get; set; }
    public IList<ReceiptModifier> Modifiers { get; set; } = new List<ReceiptModifier>();
    public string? LineNote { get; set; }
}

public class ReceiptModifier
{
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
}

public class ReceiptPayment
{
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsRefund { get; set; }
}
