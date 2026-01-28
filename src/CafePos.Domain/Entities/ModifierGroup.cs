using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class ModifierGroup
{
    public Guid ModifierGroupId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ModifierSelectionType SelectionType { get; set; }
    public int MinSelections { get; set; }
    public int? MaxSelections { get; set; }
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
    public ICollection<ModifierOption> Options { get; set; } = new List<ModifierOption>();
    public ICollection<ItemModifierGroup> ItemModifierGroups { get; set; } = new List<ItemModifierGroup>();
}
