using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;

namespace CafePos.Infrastructure.Services;

public class ReceiptNumberService : IReceiptNumberService
{
    private readonly IPosDbContext _db;

    public ReceiptNumberService(IPosDbContext db)
    {
        _db = db;
    }

    public async Task<string> NextReceiptNumberAsync(Guid storeId, Guid terminalId, DateOnly businessDate, CancellationToken cancellationToken = default)
    {
        var terminal = _db.Terminals.FirstOrDefault(t => t.TerminalId == terminalId && t.IsActive);
        var prefix = terminal?.ReceiptPrefix ?? "T1";

        var sequence = _db.ReceiptSequences.FirstOrDefault(s => s.StoreId == storeId && s.TerminalId == terminalId && s.BusinessDate == businessDate);
        if (sequence == null)
        {
            sequence = new ReceiptSequence
            {
                ReceiptSequenceId = Guid.NewGuid(),
                StoreId = storeId,
                TerminalId = terminalId,
                BusinessDate = businessDate,
                LastValue = 1
            };
            await _db.AddAsync(sequence, cancellationToken);
        }
        else
        {
            sequence.LastValue += 1;
            _db.Update(sequence);
        }

        await _db.SaveChangesAsync(cancellationToken);
        var datePart = businessDate.ToString("yyyyMMdd");
        return $"{prefix}-{datePart}-{sequence.LastValue:0000}";
    }
}
