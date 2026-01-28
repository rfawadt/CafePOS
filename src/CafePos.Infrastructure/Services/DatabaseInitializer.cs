using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;
using CafePos.Domain.Enums;
using CafePos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Infrastructure.Services;

public class DatabaseInitializer
{
    private readonly CafePosDbContext _db;
    private readonly IClock _clock;

    public DatabaseInitializer(CafePosDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);

        if (!_db.Stores.Any())
        {
            var store = new Store
            {
                StoreId = Guid.NewGuid(),
                Name = "CafePOS",
                AddressLine1 = "123 Main St",
                City = "City",
                Country = "US",
                Phone = "",
                CurrencyCode = "USD",
                Timezone = TimeZoneInfo.Local.Id,
                IsActive = true
            };
            _db.Stores.Add(store);

            var terminal = new Terminal
            {
                TerminalId = Guid.NewGuid(),
                StoreId = store.StoreId,
                Name = "Terminal 1",
                ReceiptPrefix = "T1",
                IsActive = true
            };
            _db.Terminals.Add(terminal);

            var tax = new TaxCategory
            {
                TaxCategoryId = Guid.NewGuid(),
                StoreId = store.StoreId,
                Name = "Standard",
                Rate = 0.0m,
                IsInclusive = false,
                IsActive = true
            };
            _db.TaxCategories.Add(tax);

            var cash = new PaymentMethod
            {
                PaymentMethodId = Guid.NewGuid(),
                StoreId = store.StoreId,
                Name = "Cash",
                Type = PaymentMethodType.Cash,
                IsActive = true
            };
            var card = new PaymentMethod
            {
                PaymentMethodId = Guid.NewGuid(),
                StoreId = store.StoreId,
                Name = "Card",
                Type = PaymentMethodType.External,
                IsActive = true
            };
            _db.PaymentMethods.AddRange(cash, card);

            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!_db.Roles.Any())
        {
            _db.Roles.AddRange(new Role { RoleId = Guid.NewGuid(), Name = "Cashier" },
                new Role { RoleId = Guid.NewGuid(), Name = "Manager" },
                new Role { RoleId = Guid.NewGuid(), Name = "Admin" });
            await _db.SaveChangesAsync(cancellationToken);
        }

        if (!_db.MenuCategories.Any())
        {
            var store = _db.Stores.First();
            var tax = _db.TaxCategories.FirstOrDefault(t => t.StoreId == store.StoreId);

            var coffee = new MenuCategory { CategoryId = Guid.NewGuid(), StoreId = store.StoreId, Name = "Coffee", DisplayOrder = 1, IsActive = true };
            var tea = new MenuCategory { CategoryId = Guid.NewGuid(), StoreId = store.StoreId, Name = "Tea", DisplayOrder = 2, IsActive = true };
            var pastry = new MenuCategory { CategoryId = Guid.NewGuid(), StoreId = store.StoreId, Name = "Pastries", DisplayOrder = 3, IsActive = true };
            _db.MenuCategories.AddRange(coffee, tea, pastry);

            var latte = new MenuItem { ItemId = Guid.NewGuid(), StoreId = store.StoreId, CategoryId = coffee.CategoryId, Name = "Latte", DisplayOrder = 1, IsActive = true, IsAvailable = true };
            var espresso = new MenuItem { ItemId = Guid.NewGuid(), StoreId = store.StoreId, CategoryId = coffee.CategoryId, Name = "Espresso", DisplayOrder = 2, IsActive = true, IsAvailable = true };
            var greenTea = new MenuItem { ItemId = Guid.NewGuid(), StoreId = store.StoreId, CategoryId = tea.CategoryId, Name = "Green Tea", DisplayOrder = 1, IsActive = true, IsAvailable = true };
            var croissant = new MenuItem { ItemId = Guid.NewGuid(), StoreId = store.StoreId, CategoryId = pastry.CategoryId, Name = "Croissant", DisplayOrder = 1, IsActive = true, IsAvailable = true };

            _db.MenuItems.AddRange(latte, espresso, greenTea, croissant);

            _db.MenuItemPrices.AddRange(
                new MenuItemPrice { ItemPriceId = Guid.NewGuid(), ItemId = latte.ItemId, Label = "Small", Price = 3.50m, TaxCategoryId = tax?.TaxCategoryId, DisplayOrder = 1, IsActive = true },
                new MenuItemPrice { ItemPriceId = Guid.NewGuid(), ItemId = latte.ItemId, Label = "Large", Price = 4.50m, TaxCategoryId = tax?.TaxCategoryId, DisplayOrder = 2, IsActive = true },
                new MenuItemPrice { ItemPriceId = Guid.NewGuid(), ItemId = espresso.ItemId, Label = null, Price = 2.50m, TaxCategoryId = tax?.TaxCategoryId, DisplayOrder = 1, IsActive = true },
                new MenuItemPrice { ItemPriceId = Guid.NewGuid(), ItemId = greenTea.ItemId, Label = \"Regular\", Price = 3.00m, TaxCategoryId = tax?.TaxCategoryId, DisplayOrder = 1, IsActive = true },
                new MenuItemPrice { ItemPriceId = Guid.NewGuid(), ItemId = croissant.ItemId, Label = null, Price = 2.75m, TaxCategoryId = tax?.TaxCategoryId, DisplayOrder = 1, IsActive = true }
            );

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
