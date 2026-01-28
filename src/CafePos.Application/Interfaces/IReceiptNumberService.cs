namespace CafePos.Application.Interfaces;

public interface IReceiptNumberService
{
    Task<string> NextReceiptNumberAsync(Guid storeId, Guid terminalId, DateOnly businessDate, CancellationToken cancellationToken = default);
}
