using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class Shift
{
    public Guid ShiftId { get; set; }
    public Guid StoreId { get; set; }
    public Guid TerminalId { get; set; }
    public Guid OpenedByUserId { get; set; }
    public DateTime OpenedAtLocal { get; set; }
    public decimal OpeningFloat { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public DateTime? ClosedAtLocal { get; set; }
    public decimal? CountedCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    public Terminal? Terminal { get; set; }
    public User? OpenedByUser { get; set; }
    public User? ClosedByUser { get; set; }
    public ICollection<CashDrawerEvent> CashDrawerEvents { get; set; } = new List<CashDrawerEvent>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
