using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Application.Services;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly UserService _userService;
    private readonly NavigationService _navigation;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    public SetupViewModel(UserService userService, NavigationService navigation)
    {
        _userService = userService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task CreateManagerAsync()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        try
        {
            await _userService.CreateUserAsync(Username.Trim(), string.IsNullOrWhiteSpace(DisplayName) ? Username.Trim() : DisplayName.Trim(), Password, new[] { "Manager" });
            _navigation.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
