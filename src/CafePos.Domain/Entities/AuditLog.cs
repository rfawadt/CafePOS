namespace CafePos.Domain.Entities;

public class AuditLog
{
    public Guid AuditLogId { get; set; }
    public Guid StoreId { get; set; }
    public DateTime CreatedAtLocal { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }

    public Store? Store { get; set; }
    public User? User { get; set; }
}
