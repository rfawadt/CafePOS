namespace CafePos.Domain.Entities;

public class MenuItem
{
    public Guid ItemId { get; set; }
    public Guid StoreId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? ButtonColorHex { get; set; }
    public string? ImagePath { get; set; }

    public MenuCategory? Category { get; set; }
    public ICollection<MenuItemPrice> Prices { get; set; } = new List<MenuItemPrice>();
    public ICollection<ItemModifierGroup> ItemModifierGroups { get; set; } = new List<ItemModifierGroup>();
}
