using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly NavigationService _navigation;

    public SettingsViewModel(NavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateTo<MainPosViewModel>();
    }
}
