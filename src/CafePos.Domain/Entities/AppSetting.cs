namespace CafePos.Domain.Entities;

public class AppSetting
{
    public Guid StoreId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public Store? Store { get; set; }
}
