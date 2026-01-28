using CafePos.Application.Interfaces;
using CafePos.Domain.Entities;

namespace CafePos.Application.Services;

public class MenuService
{
    private readonly IPosDbContext _db;

    public MenuService(IPosDbContext db)
    {
        _db = db;
    }

    public Task<IReadOnlyList<MenuCategory>> GetActiveMenuAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var categories = _db.MenuCategories
            .Where(c => c.StoreId == storeId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToList();

        var items = _db.MenuItems
            .Where(i => i.StoreId == storeId && i.IsActive && i.IsAvailable)
            .OrderBy(i => i.DisplayOrder)
            .ToList();

        var itemIds = items.Select(i => i.ItemId).ToList();
        var prices = _db.MenuItemPrices
            .Where(p => itemIds.Contains(p.ItemId) && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        var pricesLookup = prices.GroupBy(p => p.ItemId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var item in items)
        {
            item.Prices = pricesLookup.TryGetValue(item.ItemId, out var list) ? list : new List<MenuItemPrice>();
        }

        var itemsLookup = items.GroupBy(i => i.CategoryId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var category in categories)
        {
            category.Items = itemsLookup.TryGetValue(category.CategoryId, out var list) ? list : new List<MenuItem>();
        }

        return Task.FromResult<IReadOnlyList<MenuCategory>>(categories);
    }

    public Task<IReadOnlyList<MenuCategory>> GetMenuForManagerAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var categories = _db.MenuCategories
            .Where(c => c.StoreId == storeId)
            .OrderBy(c => c.DisplayOrder)
            .ToList();

        var items = _db.MenuItems
            .Where(i => i.StoreId == storeId)
            .OrderBy(i => i.DisplayOrder)
            .ToList();

        var itemIds = items.Select(i => i.ItemId).ToList();
        var prices = _db.MenuItemPrices
            .Where(p => itemIds.Contains(p.ItemId))
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        var pricesLookup = prices.GroupBy(p => p.ItemId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var item in items)
        {
            item.Prices = pricesLookup.TryGetValue(item.ItemId, out var list) ? list : new List<MenuItemPrice>();
        }

        var itemsLookup = items.GroupBy(i => i.CategoryId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var category in categories)
        {
            category.Items = itemsLookup.TryGetValue(category.CategoryId, out var list) ? list : new List<MenuItem>();
        }

        return Task.FromResult<IReadOnlyList<MenuCategory>>(categories);
    }

    public async Task<MenuCategory> CreateCategoryAsync(Guid storeId, string name, int displayOrder, string? colorHex, CancellationToken cancellationToken = default)
    {
        var category = new MenuCategory
        {
            CategoryId = Guid.NewGuid(),
            StoreId = storeId,
            Name = name,
            DisplayOrder = displayOrder,
            ColorHex = colorHex,
            IsActive = true
        };
        await _db.AddAsync(category, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<MenuItem> CreateItemAsync(Guid storeId, Guid categoryId, string name, string? description, int displayOrder, string? buttonColorHex, CancellationToken cancellationToken = default)
    {
        var item = new MenuItem
        {
            ItemId = Guid.NewGuid(),
            StoreId = storeId,
            CategoryId = categoryId,
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            ButtonColorHex = buttonColorHex,
            IsActive = true,
            IsAvailable = true
        };
        await _db.AddAsync(item, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<MenuItemPrice> CreatePriceAsync(Guid itemId, string? label, decimal price, Guid? taxCategoryId, int displayOrder, CancellationToken cancellationToken = default)
    {
        var priceEntity = new MenuItemPrice
        {
            ItemPriceId = Guid.NewGuid(),
            ItemId = itemId,
            Label = label,
            Price = price,
            TaxCategoryId = taxCategoryId,
            DisplayOrder = displayOrder,
            IsActive = true
        };
        await _db.AddAsync(priceEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return priceEntity;
    }
}
