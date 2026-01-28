namespace CafePos.Domain.Entities;

public class OrderLineModifier
{
    public Guid LineModifierId { get; set; }
    public Guid OrderLineId { get; set; }
    public string ModifierGroupNameSnapshot { get; set; } = string.Empty;
    public string ModifierOptionNameSnapshot { get; set; } = string.Empty;
    public decimal PriceDeltaSnapshot { get; set; }

    public OrderLine? OrderLine { get; set; }
}
