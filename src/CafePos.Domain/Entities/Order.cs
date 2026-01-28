using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class Order
{
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid? ShiftId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Open;
    public OrderType OrderType { get; set; } = OrderType.Takeaway;
    public decimal Subtotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal ChangeDue { get; set; }
    public DateTime CreatedAtLocal { get; set; }
    public DateTime? CompletedAtLocal { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? Notes { get; set; }
    public string? HeldName { get; set; }

    public Terminal? Terminal { get; set; }
    public Shift? Shift { get; set; }
    public User? CreatedByUser { get; set; }
    public User? CompletedByUser { get; set; }
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
