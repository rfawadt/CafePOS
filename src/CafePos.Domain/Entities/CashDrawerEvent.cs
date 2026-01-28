using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class CashDrawerEvent
{
    public Guid CashDrawerEventId { get; set; }
    public Guid ShiftId { get; set; }
    public CashDrawerEventType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtLocal { get; set; }
    public Guid UserId { get; set; }

    public Shift? Shift { get; set; }
    public User? User { get; set; }
}
