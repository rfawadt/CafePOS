using CafePos.Domain.Entities;

namespace CafePos.Application.Interfaces;

public interface IPosDbContext
{
    IQueryable<Store> Stores { get; }
    IQueryable<Terminal> Terminals { get; }
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<MenuCategory> MenuCategories { get; }
    IQueryable<MenuItem> MenuItems { get; }
    IQueryable<MenuItemPrice> MenuItemPrices { get; }
    IQueryable<ModifierGroup> ModifierGroups { get; }
    IQueryable<ModifierOption> ModifierOptions { get; }
    IQueryable<ItemModifierGroup> ItemModifierGroups { get; }
    IQueryable<TaxCategory> TaxCategories { get; }
    IQueryable<Order> Orders { get; }
    IQueryable<OrderLine> OrderLines { get; }
    IQueryable<OrderLineModifier> OrderLineModifiers { get; }
    IQueryable<PaymentMethod> PaymentMethods { get; }
    IQueryable<Payment> Payments { get; }
    IQueryable<Shift> Shifts { get; }
    IQueryable<CashDrawerEvent> CashDrawerEvents { get; }
    IQueryable<AppSetting> AppSettings { get; }
    IQueryable<AuditLog> AuditLogs { get; }
    IQueryable<ReceiptSequence> ReceiptSequences { get; }

    Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
