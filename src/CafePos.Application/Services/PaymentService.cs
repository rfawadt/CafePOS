using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;

namespace CafePos.Application.Services;

public class PaymentService
{
    private readonly IPosDbContext _db;

    public PaymentService(IPosDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<PaymentMethod>> GetActivePaymentMethodsAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var methods = _db.PaymentMethods.Where(pm => pm.StoreId == storeId && pm.IsActive).ToList();
        return Task.FromResult<IReadOnlyList<PaymentMethod>>(methods);
    }
}
