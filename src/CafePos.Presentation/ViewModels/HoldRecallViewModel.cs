using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Models;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class HoldRecallViewModel : ObservableObject
{
    private readonly OrderQueryService _orderQueryService;
    private readonly OrderService _orderService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    [ObservableProperty]
    private ObservableCollection<HeldOrderDto> _heldOrders = new();

    [ObservableProperty]
    private HeldOrderDto? _selectedOrder;

    [ObservableProperty]
    private string _errorMessage = "";

    public HoldRecallViewModel(OrderQueryService orderQueryService, OrderService orderService, NavigationService navigation, SessionState session)
    {
        _orderQueryService = orderQueryService;
        _orderService = orderService;
        _navigation = navigation;
        _session = session;
        _ = LoadHeldOrdersAsync();
    }

    private async Task LoadHeldOrdersAsync()
    {
        var orders = await _orderQueryService.GetHeldOrdersAsync(_session.TerminalId);
        HeldOrders = new ObservableCollection<HeldOrderDto>(orders);
    }

    [RelayCommand]
    private async Task RecallAsync(HeldOrderDto? order)
    {
        order ??= SelectedOrder;
        if (order == null)
        {
            return;
        }

        await _orderService.RecallOrderAsync(order.OrderId);
        _session.CurrentOrderId = order.OrderId;
        _navigation.NavigateTo<MainPosViewModel>();
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateTo<MainPosViewModel>();
    }
}
