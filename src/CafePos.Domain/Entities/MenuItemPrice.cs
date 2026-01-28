namespace CafePos.Domain.Entities;

public class MenuItemPrice
{
    public Guid ItemPriceId { get; set; }
    public Guid ItemId { get; set; }
    public string? Label { get; set; }
    public decimal Price { get; set; }
    public Guid? TaxCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public MenuItem? Item { get; set; }
    public TaxCategory? TaxCategory { get; set; }
}
