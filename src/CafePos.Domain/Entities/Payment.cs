namespace CafePos.Domain.Entities;

public class Payment
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CapturedAtLocal { get; set; }
    public string? Reference { get; set; }
    public bool IsRefund { get; set; }

    public Order? Order { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
}
