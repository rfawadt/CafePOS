namespace CafePos.Domain.Entities;

public class Store
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";
    public string Timezone { get; set; } = "America/New_York";
    public bool IsActive { get; set; } = true;
}
