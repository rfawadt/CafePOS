using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;
using CafePos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Infrastructure.Persistence;

public class CafePosDbContext : DbContext, IPosDbContext
{
    public CafePosDbContext(DbContextOptions<CafePosDbContext> options) : base(options)
    {
    }

    public DbSet<Store> StoresSet => Set<Store>();
    public DbSet<Terminal> TerminalsSet => Set<Terminal>();
    public DbSet<User> UsersSet => Set<User>();
    public DbSet<Role> RolesSet => Set<Role>();
    public DbSet<UserRole> UserRolesSet => Set<UserRole>();
    public DbSet<MenuCategory> MenuCategoriesSet => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItemsSet => Set<MenuItem>();
    public DbSet<MenuItemPrice> MenuItemPricesSet => Set<MenuItemPrice>();
    public DbSet<ModifierGroup> ModifierGroupsSet => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptionsSet => Set<ModifierOption>();
    public DbSet<ItemModifierGroup> ItemModifierGroupsSet => Set<ItemModifierGroup>();
    public DbSet<TaxCategory> TaxCategoriesSet => Set<TaxCategory>();
    public DbSet<Order> OrdersSet => Set<Order>();
    public DbSet<OrderLine> OrderLinesSet => Set<OrderLine>();
    public DbSet<OrderLineModifier> OrderLineModifiersSet => Set<OrderLineModifier>();
    public DbSet<PaymentMethod> PaymentMethodsSet => Set<PaymentMethod>();
    public DbSet<Payment> PaymentsSet => Set<Payment>();
    public DbSet<Shift> ShiftsSet => Set<Shift>();
    public DbSet<CashDrawerEvent> CashDrawerEventsSet => Set<CashDrawerEvent>();
    public DbSet<AppSetting> AppSettingsSet => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogsSet => Set<AuditLog>();
    public DbSet<ReceiptSequence> ReceiptSequencesSet => Set<ReceiptSequence>();

    IQueryable<Store> IPosDbContext.Stores => StoresSet;
    IQueryable<Terminal> IPosDbContext.Terminals => TerminalsSet;
    IQueryable<User> IPosDbContext.Users => UsersSet;
    IQueryable<Role> IPosDbContext.Roles => RolesSet;
    IQueryable<UserRole> IPosDbContext.UserRoles => UserRolesSet;
    IQueryable<MenuCategory> IPosDbContext.MenuCategories => MenuCategoriesSet;
    IQueryable<MenuItem> IPosDbContext.MenuItems => MenuItemsSet;
    IQueryable<MenuItemPrice> IPosDbContext.MenuItemPrices => MenuItemPricesSet;
    IQueryable<ModifierGroup> IPosDbContext.ModifierGroups => ModifierGroupsSet;
    IQueryable<ModifierOption> IPosDbContext.ModifierOptions => ModifierOptionsSet;
    IQueryable<ItemModifierGroup> IPosDbContext.ItemModifierGroups => ItemModifierGroupsSet;
    IQueryable<TaxCategory> IPosDbContext.TaxCategories => TaxCategoriesSet;
    IQueryable<Order> IPosDbContext.Orders => OrdersSet;
    IQueryable<OrderLine> IPosDbContext.OrderLines => OrderLinesSet;
    IQueryable<OrderLineModifier> IPosDbContext.OrderLineModifiers => OrderLineModifiersSet;
    IQueryable<PaymentMethod> IPosDbContext.PaymentMethods => PaymentMethodsSet;
    IQueryable<Payment> IPosDbContext.Payments => PaymentsSet;
    IQueryable<Shift> IPosDbContext.Shifts => ShiftsSet;
    IQueryable<CashDrawerEvent> IPosDbContext.CashDrawerEvents => CashDrawerEventsSet;
    IQueryable<AppSetting> IPosDbContext.AppSettings => AppSettingsSet;
    IQueryable<AuditLog> IPosDbContext.AuditLogs => AuditLogsSet;
    IQueryable<ReceiptSequence> IPosDbContext.ReceiptSequences => ReceiptSequencesSet;

    public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
    {
        return Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Update(entity);
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await Database.BeginTransactionAsync(cancellationToken);
        return new EfTransaction(tx);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Store>().HasKey(x => x.StoreId);
        modelBuilder.Entity<Terminal>().HasKey(x => x.TerminalId);
        modelBuilder.Entity<User>().HasKey(x => x.UserId);
        modelBuilder.Entity<Role>().HasKey(x => x.RoleId);
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        modelBuilder.Entity<MenuCategory>().HasKey(x => x.CategoryId);
        modelBuilder.Entity<MenuItem>().HasKey(x => x.ItemId);
        modelBuilder.Entity<MenuItemPrice>().HasKey(x => x.ItemPriceId);
        modelBuilder.Entity<ModifierGroup>().HasKey(x => x.ModifierGroupId);
        modelBuilder.Entity<ModifierOption>().HasKey(x => x.ModifierOptionId);
        modelBuilder.Entity<ItemModifierGroup>().HasKey(x => new { x.ItemId, x.ModifierGroupId });
        modelBuilder.Entity<TaxCategory>().HasKey(x => x.TaxCategoryId);

        modelBuilder.Entity<Order>().HasKey(x => x.OrderId);
        modelBuilder.Entity<OrderLine>().HasKey(x => x.OrderLineId);
        modelBuilder.Entity<OrderLineModifier>().HasKey(x => x.LineModifierId);
        modelBuilder.Entity<PaymentMethod>().HasKey(x => x.PaymentMethodId);
        modelBuilder.Entity<Payment>().HasKey(x => x.PaymentId);
        modelBuilder.Entity<Shift>().HasKey(x => x.ShiftId);
        modelBuilder.Entity<CashDrawerEvent>().HasKey(x => x.CashDrawerEventId);
        modelBuilder.Entity<AppSetting>().HasKey(x => new { x.StoreId, x.Key });
        modelBuilder.Entity<AuditLog>().HasKey(x => x.AuditLogId);
        modelBuilder.Entity<ReceiptSequence>().HasKey(x => x.ReceiptSequenceId);

        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => x.ReceiptNumber).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => x.CreatedAtLocal);
        modelBuilder.Entity<Order>().HasIndex(x => x.CompletedAtLocal);

        modelBuilder.Entity<ReceiptSequence>().HasIndex(x => new { x.StoreId, x.TerminalId, x.BusinessDate }).IsUnique();

        ConfigureRelationships(modelBuilder);
        ConfigurePrecision(modelBuilder);
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Terminal>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<MenuCategory>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<MenuItem>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CategoryId);

        modelBuilder.Entity<MenuItemPrice>()
            .HasOne(x => x.Item)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.ItemId);

        modelBuilder.Entity<MenuItemPrice>()
            .HasOne(x => x.TaxCategory)
            .WithMany()
            .HasForeignKey(x => x.TaxCategoryId);

        modelBuilder.Entity<ModifierGroup>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<ModifierOption>()
            .HasOne(x => x.ModifierGroup)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.ModifierGroupId);

        modelBuilder.Entity<ItemModifierGroup>()
            .HasOne(x => x.Item)
            .WithMany(x => x.ItemModifierGroups)
            .HasForeignKey(x => x.ItemId);

        modelBuilder.Entity<ItemModifierGroup>()
            .HasOne(x => x.ModifierGroup)
            .WithMany(x => x.ItemModifierGroups)
            .HasForeignKey(x => x.ModifierGroupId);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Terminal)
            .WithMany()
            .HasForeignKey(x => x.TerminalId);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Shift)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderLine>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.OrderId);

        modelBuilder.Entity<OrderLine>()
            .HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderLine>()
            .HasOne(x => x.ItemPrice)
            .WithMany()
            .HasForeignKey(x => x.ItemPriceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderLineModifier>()
            .HasOne(x => x.OrderLine)
            .WithMany(x => x.Modifiers)
            .HasForeignKey(x => x.OrderLineId);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.OrderId);

        modelBuilder.Entity<Payment>()
            .HasOne(x => x.PaymentMethod)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId);

        modelBuilder.Entity<Shift>()
            .HasOne(x => x.Terminal)
            .WithMany()
            .HasForeignKey(x => x.TerminalId);

        modelBuilder.Entity<Shift>()
            .HasOne(x => x.OpenedByUser)
            .WithMany()
            .HasForeignKey(x => x.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shift>()
            .HasOne(x => x.ClosedByUser)
            .WithMany()
            .HasForeignKey(x => x.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CashDrawerEvent>()
            .HasOne(x => x.Shift)
            .WithMany(x => x.CashDrawerEvents)
            .HasForeignKey(x => x.ShiftId);

        modelBuilder.Entity<CashDrawerEvent>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppSetting>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReceiptSequence>()
            .HasOne(x => x.Store)
            .WithMany()
            .HasForeignKey(x => x.StoreId);

        modelBuilder.Entity<ReceiptSequence>()
            .HasOne(x => x.Terminal)
            .WithMany()
            .HasForeignKey(x => x.TerminalId);
    }

    private static void ConfigurePrecision(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(decimal))
                {
                    if (property.Name == nameof(TaxCategory.Rate) || property.Name == nameof(OrderLine.TaxRateSnapshot))
                    {
                        property.SetPrecision(9);
                        property.SetScale(4);
                    }
                    else
                    {
                        property.SetPrecision(18);
                        property.SetScale(2);
                    }
                }
            }
        }
    }
}
