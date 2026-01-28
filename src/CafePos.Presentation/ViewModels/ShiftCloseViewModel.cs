using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Models;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class ShiftCloseViewModel : ObservableObject
{
    private readonly ShiftService _shiftService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    [ObservableProperty]
    private ShiftSummaryDto? _summary;

    [ObservableProperty]
    private decimal _countedCash;

    [ObservableProperty]
    private string _errorMessage = "";

    public ShiftCloseViewModel(ShiftService shiftService, NavigationService navigation, SessionState session)
    {
        _shiftService = shiftService;
        _navigation = navigation;
        _session = session;
        _ = LoadSummaryAsync();
    }

    private async Task LoadSummaryAsync()
    {
        if (_session.OpenShiftId == null)
        {
            return;
        }

        Summary = await _shiftService.GetShiftSummaryAsync(_session.OpenShiftId.Value);
    }

    [RelayCommand]
    private async Task CloseShiftAsync()
    {
        ErrorMessage = "";
        if (_session.OpenShiftId == null || _session.CurrentUserId == null)
        {
            ErrorMessage = "No open shift.";
            return;
        }

        try
        {
            Summary = await _shiftService.CloseShiftAsync(_session.OpenShiftId.Value, _session.CurrentUserId.Value, CountedCash);
            _session.OpenShiftId = null;
            _navigation.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateTo<MainPosViewModel>();
    }
}
