using CommunityToolkit.Mvvm.ComponentModel;
using CafePos.Application.Interfaces;
using CafePos.Presentation.Services;

namespace CafePos.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public NavigationService Navigation { get; }
    private readonly IPosDbContext _db;
    private readonly SessionState _session;

    public MainViewModel(NavigationService navigation, IPosDbContext db, SessionState session)
    {
        Navigation = navigation;
        _db = db;
        _session = session;
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        var store = _db.Stores.FirstOrDefault(s => s.IsActive);
        var terminal = _db.Terminals.FirstOrDefault(t => t.IsActive);
        if (store != null) _session.StoreId = store.StoreId;
        if (terminal != null) _session.TerminalId = terminal.TerminalId;

        var hasUsers = _db.Users.Any();
        if (!hasUsers)
        {
            Navigation.NavigateTo<SetupViewModel>();
        }
        else
        {
            Navigation.NavigateTo<LoginViewModel>();
        }

        await Task.CompletedTask;
    }
}
