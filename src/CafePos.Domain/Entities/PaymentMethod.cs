using CafePos.Domain.Enums;

namespace CafePos.Domain.Entities;

public class PaymentMethod
{
    public Guid PaymentMethodId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public PaymentMethodType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
}
