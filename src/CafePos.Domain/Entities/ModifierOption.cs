namespace CafePos.Domain.Entities;

public class ModifierOption
{
    public Guid ModifierOptionId { get; set; }
    public Guid ModifierGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ModifierGroup? ModifierGroup { get; set; }
}
