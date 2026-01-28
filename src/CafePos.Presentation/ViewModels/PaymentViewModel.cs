using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Models;
using CafePos.Application.Services;
using CafePos.Presentation.Services;
using CafePos.Domain.Enums;

namespace CafePos.Presentation.ViewModels;

public partial class PaymentViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly OrderQueryService _orderQueryService;
    private readonly PaymentService _paymentService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    private Guid? _cashMethodId;
    private Guid? _cardMethodId;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _cashReceived;

    [ObservableProperty]
    private decimal _cardAmount;

    [ObservableProperty]
    private decimal _changeDue;

    [ObservableProperty]
    private string _errorMessage = "";

    public PaymentViewModel(OrderService orderService, OrderQueryService orderQueryService, PaymentService paymentService, NavigationService navigation, SessionState session)
    {
        _orderService = orderService;
        _orderQueryService = orderQueryService;
        _paymentService = paymentService;
        _navigation = navigation;
        _session = session;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        if (_session.CurrentOrderId == null)
        {
            return;
        }

        var order = await _orderQueryService.GetOrderDetailsAsync(_session.CurrentOrderId.Value);
        if (order != null)
        {
            Total = order.Total;
        }

        var methods = await _paymentService.GetActivePaymentMethodsAsync(_session.StoreId);
        _cashMethodId = methods.FirstOrDefault(m => m.Type == PaymentMethodType.Cash)?.PaymentMethodId;
        _cardMethodId = methods.FirstOrDefault(m => m.Name.Equals("Card", StringComparison.OrdinalIgnoreCase) || m.Type == PaymentMethodType.External)?.PaymentMethodId;
    }

    [RelayCommand]
    private async Task PayCashAsync()
    {
        if (_session.CurrentOrderId == null || _session.CurrentUserId == null)
        {
            return;
        }

        if (_cashMethodId == null)
        {
            ErrorMessage = "Cash payment method not configured.";
            return;
        }

        var amount = CashReceived <= 0 ? Total : CashReceived;
        ChangeDue = Math.Max(0m, amount - Total);

        await CompleteAsync(new[] { new PaymentInput(_cashMethodId.Value, amount, null) });
    }

    [RelayCommand]
    private async Task PayCardAsync()
    {
        if (_session.CurrentOrderId == null || _session.CurrentUserId == null)
        {
            return;
        }

        if (_cardMethodId == null)
        {
            ErrorMessage = "Card payment method not configured.";
            return;
        }

        await CompleteAsync(new[] { new PaymentInput(_cardMethodId.Value, Total, null) });
    }

    [RelayCommand]
    private async Task PaySplitAsync()
    {
        if (_session.CurrentOrderId == null || _session.CurrentUserId == null)
        {
            return;
        }

        if (_cashMethodId == null || _cardMethodId == null)
        {
            ErrorMessage = "Payment methods not configured.";
            return;
        }

        var payments = new List<PaymentInput>();
        if (CashReceived > 0)
        {
            payments.Add(new PaymentInput(_cashMethodId.Value, CashReceived, null));
        }
        if (CardAmount > 0)
        {
            payments.Add(new PaymentInput(_cardMethodId.Value, CardAmount, null));
        }

        if (payments.Count == 0)
        {
            ErrorMessage = "Enter cash and/or card amount.";
            return;
        }

        await CompleteAsync(payments);
    }

    private async Task CompleteAsync(IReadOnlyList<PaymentInput> payments)
    {
        ErrorMessage = "";
        try
        {
            await _orderService.CompleteOrderAsync(_session.CurrentOrderId!.Value, _session.TerminalId, _session.CurrentUserId!.Value, payments);
            _session.CurrentOrderId = null;
            _navigation.NavigateTo<MainPosViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void PayBack()
    {
        _navigation.NavigateTo<MainPosViewModel>();
    }
}
