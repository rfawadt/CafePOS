using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class OpenShiftViewModel : ObservableObject
{
    private readonly ShiftService _shiftService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    [ObservableProperty]
    private decimal _openingFloat;

    [ObservableProperty]
    private string _errorMessage = "";

    public OpenShiftViewModel(ShiftService shiftService, NavigationService navigation, SessionState session)
    {
        _shiftService = shiftService;
        _navigation = navigation;
        _session = session;
    }

    [RelayCommand]
    private async Task OpenShiftAsync()
    {
        ErrorMessage = "";
        if (_session.CurrentUserId == null)
        {
            ErrorMessage = "No user logged in.";
            return;
        }

        try
        {
            var shift = await _shiftService.OpenShiftAsync(_session.StoreId, _session.TerminalId, _session.CurrentUserId.Value, OpeningFloat);
            _session.OpenShiftId = shift.ShiftId;
            _navigation.NavigateTo<MainPosViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
