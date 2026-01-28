using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly ShiftService _shiftService;
    private readonly NavigationService _navigation;
    private readonly SessionState _session;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    public LoginViewModel(AuthService authService, ShiftService shiftService, NavigationService navigation, SessionState session)
    {
        _authService = authService;
        _shiftService = shiftService;
        _navigation = navigation;
        _session = session;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = "";
        var result = await _authService.LoginAsync(Username, Password);
        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return;
        }

        _session.CurrentUserId = result.UserId;
        _session.DisplayName = result.DisplayName;
        _session.Roles = result.Roles.ToList();

        var openShift = await _shiftService.GetOpenShiftAsync(_session.TerminalId);
        if (openShift == null)
        {
            _navigation.NavigateTo<OpenShiftViewModel>();
        }
        else
        {
            _session.OpenShiftId = openShift.ShiftId;
            _navigation.NavigateTo<MainPosViewModel>();
        }
    }
}
