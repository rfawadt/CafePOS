using CommunityToolkit.Mvvm.ComponentModel;

namespace CafePos.Presentation.ViewModels;

public partial class MenuCategoryViewModel : ObservableObject
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public partial class MenuItemButtonViewModel : ObservableObject
{
    public Guid ItemPriceId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public partial class CartLineViewModel : ObservableObject
{
    public Guid OrderLineId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal Qty { get; init; }
    public decimal LineTotal { get; init; }
    public string? LineNote { get; init; }
}
