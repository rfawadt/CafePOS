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

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Terminal> Terminals => Set<Terminal>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemPrice> MenuItemPrices => Set<MenuItemPrice>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<ItemModifierGroup> ItemModifierGroups => Set<ItemModifierGroup>();
    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderLineModifier> OrderLineModifiers => Set<OrderLineModifier>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<CashDrawerEvent> CashDrawerEvents => Set<CashDrawerEvent>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ReceiptSequence> ReceiptSequences => Set<ReceiptSequence>();

    IQueryable<Store> IPosDbContext.Stores => Stores;
    IQueryable<Terminal> IPosDbContext.Terminals => Terminals;
    IQueryable<User> IPosDbContext.Users => Users;
    IQueryable<Role> IPosDbContext.Roles => Roles;
    IQueryable<UserRole> IPosDbContext.UserRoles => UserRoles;
    IQueryable<MenuCategory> IPosDbContext.MenuCategories => MenuCategories;
    IQueryable<MenuItem> IPosDbContext.MenuItems => MenuItems;
    IQueryable<MenuItemPrice> IPosDbContext.MenuItemPrices => MenuItemPrices;
    IQueryable<ModifierGroup> IPosDbContext.ModifierGroups => ModifierGroups;
    IQueryable<ModifierOption> IPosDbContext.ModifierOptions => ModifierOptions;
    IQueryable<ItemModifierGroup> IPosDbContext.ItemModifierGroups => ItemModifierGroups;
    IQueryable<TaxCategory> IPosDbContext.TaxCategories => TaxCategories;
    IQueryable<Order> IPosDbContext.Orders => Orders;
    IQueryable<OrderLine> IPosDbContext.OrderLines => OrderLines;
    IQueryable<OrderLineModifier> IPosDbContext.OrderLineModifiers => OrderLineModifiers;
    IQueryable<PaymentMethod> IPosDbContext.PaymentMethods => PaymentMethods;
    IQueryable<Payment> IPosDbContext.Payments => Payments;
    IQueryable<Shift> IPosDbContext.Shifts => Shifts;
    IQueryable<CashDrawerEvent> IPosDbContext.CashDrawerEvents => CashDrawerEvents;
    IQueryable<AppSetting> IPosDbContext.AppSettings => AppSettings;
    IQueryable<AuditLog> IPosDbContext.AuditLogs => AuditLogs;
    IQueryable<ReceiptSequence> IPosDbContext.ReceiptSequences => ReceiptSequences;

    Task IPosDbContext.AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken)
        where TEntity : class
    {
        return Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    Task IPosDbContext.AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        where TEntity : class
    {
        return Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    void IPosDbContext.Update<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Update(entity);
    }

    void IPosDbContext.Remove<TEntity>(TEntity entity) where TEntity : class
    {
        Set<TEntity>().Remove(entity);
    }

    Task<int> IPosDbContext.SaveChangesAsync(CancellationToken cancellationToken)
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
