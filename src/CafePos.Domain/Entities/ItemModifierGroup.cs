namespace CafePos.Domain.Entities;

public class ItemModifierGroup
{
    public Guid ItemId { get; set; }
    public Guid ModifierGroupId { get; set; }

    public MenuItem? Item { get; set; }
    public ModifierGroup? ModifierGroup { get; set; }
}
