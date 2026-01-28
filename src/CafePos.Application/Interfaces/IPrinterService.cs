using CafePos.Application.Models;

namespace CafePos.Application.Interfaces;

public interface IPrinterService
{
    Task PrintReceiptAsync(ReceiptData data, CancellationToken cancellationToken = default);
}
