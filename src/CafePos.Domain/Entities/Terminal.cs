namespace CafePos.Domain.Entities;

public class Terminal
{
    public Guid TerminalId { get; set; }
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReceiptPrefix { get; set; } = "T1";
    public bool IsActive { get; set; } = true;

    public Store? Store { get; set; }
}
