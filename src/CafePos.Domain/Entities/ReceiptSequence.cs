namespace CafePos.Domain.Entities;

public class ReceiptSequence
{
    public Guid ReceiptSequenceId { get; set; }
    public Guid StoreId { get; set; }
    public Guid TerminalId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public int LastValue { get; set; }

    public Store? Store { get; set; }
    public Terminal? Terminal { get; set; }
}
