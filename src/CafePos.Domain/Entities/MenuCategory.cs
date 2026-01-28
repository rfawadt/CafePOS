namespace CafePos.Domain.Entities;

public class MenuCategory
{
    public Guid CategoryId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string? ColorHex { get; set; }
    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
