namespace CafePos.Domain.Entities;

public class OrderLine
{
    public Guid OrderLineId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? ItemPriceId { get; set; }
    public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal UnitPriceSnapshot { get; set; }
    public decimal Qty { get; set; } = 1m;
    public decimal LineDiscount { get; set; }
    public decimal TaxRateSnapshot { get; set; }
    public bool IsTaxInclusiveSnapshot { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? LineNote { get; set; }

    public Order? Order { get; set; }
    public MenuItem? Item { get; set; }
    public MenuItemPrice? ItemPrice { get; set; }
    public ICollection<OrderLineModifier> Modifiers { get; set; } = new List<OrderLineModifier>();
}
