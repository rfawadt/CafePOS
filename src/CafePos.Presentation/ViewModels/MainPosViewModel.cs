using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class MainPosViewModel : ObservableObject
{
    private readonly MenuService _menuService;
    private readonly OrderService _orderService;
    private readonly OrderQueryService _orderQueryService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    [ObservableProperty]
    private ObservableCollection<MenuCategoryViewModel> _categories = new();

    [ObservableProperty]
    private MenuCategoryViewModel? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<MenuItemButtonViewModel> _visibleItems = new();

    [ObservableProperty]
    private ObservableCollection<CartLineViewModel> _cartLines = new();

    [ObservableProperty]
    private CartLineViewModel? _selectedCartLine;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxTotal;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _statusMessage = "";

    public MainPosViewModel(MenuService menuService, OrderService orderService, OrderQueryService orderQueryService, NavigationService navigation, SessionState session)
    {
        _menuService = menuService;
        _orderService = orderService;
        _orderQueryService = orderQueryService;
        _navigation = navigation;
        _session = session;
        _ = InitializeAsync();
    }

    partial void OnSelectedCategoryChanged(MenuCategoryViewModel? value)
    {
        _ = LoadItemsAsync();
    }

    private async Task InitializeAsync()
    {
        await EnsureOrderAsync();
        await LoadMenuAsync();
        await LoadOrderAsync();
    }

    private async Task LoadMenuAsync()
    {
        var categories = await _menuService.GetActiveMenuAsync(_session.StoreId);
        Categories = new ObservableCollection<MenuCategoryViewModel>(categories.Select(c => new MenuCategoryViewModel
        {
            CategoryId = c.CategoryId,
            Name = c.Name
        }));
        SelectedCategory = Categories.FirstOrDefault();
    }

    private async Task LoadItemsAsync()
    {
        if (SelectedCategory == null)
        {
            VisibleItems = new ObservableCollection<MenuItemButtonViewModel>();
            return;
        }

        var categories = await _menuService.GetActiveMenuAsync(_session.StoreId);
        var category = categories.FirstOrDefault(c => c.CategoryId == SelectedCategory.CategoryId);
        if (category == null)
        {
            VisibleItems = new ObservableCollection<MenuItemButtonViewModel>();
            return;
        }

        var items = new List<MenuItemButtonViewModel>();
        foreach (var item in category.Items.OrderBy(i => i.DisplayOrder))
        {
            foreach (var price in item.Prices.OrderBy(p => p.DisplayOrder))
            {
                var label = string.IsNullOrWhiteSpace(price.Label) ? item.Name : $"{item.Name} - {price.Label}";
                items.Add(new MenuItemButtonViewModel
                {
                    ItemPriceId = price.ItemPriceId,
                    DisplayName = label,
                    Price = price.Price
                });
            }
        }

        VisibleItems = new ObservableCollection<MenuItemButtonViewModel>(items);
    }

    private async Task EnsureOrderAsync()
    {
        if (_session.CurrentOrderId != null)
        {
            return;
        }

        if (_session.CurrentUserId == null)
        {
            _navigation.NavigateTo<LoginViewModel>();
            return;
        }

        var order = await _orderService.StartOrderAsync(_session.StoreId, _session.TerminalId, _session.CurrentUserId.Value);
        _session.CurrentOrderId = order.OrderId;
    }

    private async Task LoadOrderAsync()
    {
        if (_session.CurrentOrderId == null)
        {
            return;
        }

        var orderModel = await _orderQueryService.GetOrderDetailsAsync(_session.CurrentOrderId.Value);
        if (orderModel == null)
        {
            return;
        }

        CartLines = new ObservableCollection<CartLineViewModel>(orderModel.Lines.Select(l => new CartLineViewModel
        {
            OrderLineId = l.OrderLineId,
            Description = l.Description,
            Qty = l.Qty,
            LineTotal = l.LineTotal,
            LineNote = l.LineNote
        }));

        Subtotal = orderModel.Subtotal;
        TaxTotal = orderModel.TaxTotal;
        Total = orderModel.Total;
    }

    [RelayCommand]
    private async Task AddItemAsync(MenuItemButtonViewModel? item)
    {
        if (item == null || _session.CurrentOrderId == null)
        {
            return;
        }

        await _orderService.AddItemToOrderAsync(_session.CurrentOrderId.Value, item.ItemPriceId, Array.Empty<Guid>(), null);
        await LoadOrderAsync();
    }

    [RelayCommand]
    private async Task IncreaseQtyAsync()
    {
        if (SelectedCartLine == null)
        {
            return;
        }

        await _orderService.UpdateLineQtyAsync(SelectedCartLine.OrderLineId, SelectedCartLine.Qty + 1);
        await LoadOrderAsync();
    }

    [RelayCommand]
    private async Task DecreaseQtyAsync()
    {
        if (SelectedCartLine == null)
        {
            return;
        }

        var newQty = SelectedCartLine.Qty - 1;
        if (newQty <= 0)
        {
            await _orderService.RemoveLineAsync(SelectedCartLine.OrderLineId);
        }
        else
        {
            await _orderService.UpdateLineQtyAsync(SelectedCartLine.OrderLineId, newQty);
        }

        await LoadOrderAsync();
    }

    [RelayCommand]
    private async Task RemoveLineAsync()
    {
        if (SelectedCartLine == null)
        {
            return;
        }

        await _orderService.RemoveLineAsync(SelectedCartLine.OrderLineId);
        await LoadOrderAsync();
    }

    [RelayCommand]
    private async Task NewOrderAsync()
    {
        _session.CurrentOrderId = null;
        await EnsureOrderAsync();
        await LoadOrderAsync();
    }

    [RelayCommand]
    private async Task HoldOrderAsync()
    {
        if (_session.CurrentOrderId == null)
        {
            return;
        }

        await _orderService.HoldOrderAsync(_session.CurrentOrderId.Value, "Held");
        _session.CurrentOrderId = null;
        await EnsureOrderAsync();
        await LoadOrderAsync();
    }

    [RelayCommand]
    private void RecallOrder()
    {
        _navigation.NavigateTo<HoldRecallViewModel>();
    }

    [RelayCommand]
    private void Pay()
    {
        _navigation.NavigateTo<PaymentViewModel>();
    }

    [RelayCommand]
    private void CloseShift()
    {
        _navigation.NavigateTo<ShiftCloseViewModel>();
    }

    [RelayCommand]
    private void OpenReports()
    {
        _navigation.NavigateTo<ReportsViewModel>();
    }

    [RelayCommand]
    private void OpenMenuSetup()
    {
        _navigation.NavigateTo<MenuSetupViewModel>();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        _navigation.NavigateTo<SettingsViewModel>();
    }

    [RelayCommand]
    private void OpenUsers()
    {
        _navigation.NavigateTo<UsersViewModel>();
    }
}
