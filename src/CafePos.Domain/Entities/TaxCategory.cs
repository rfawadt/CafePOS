using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class TaxCategory
{
    public Guid TaxCategoryId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
    public TaxRoundingMode RoundingMode { get; set; } = TaxRoundingMode.LineLevel;
    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
}
