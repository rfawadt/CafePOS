using CommunityToolkit.Mvvm.ComponentModel;

namespace CafePos.Presentation.Services;

public partial class SessionState : ObservableObject
{
    [ObservableProperty]
    private Guid _storeId;

    [ObservableProperty]
    private Guid _terminalId;

    [ObservableProperty]
    private Guid? _currentUserId;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private IList<string> _roles = new List<string>();

    [ObservableProperty]
    private Guid? _openShiftId;

    [ObservableProperty]
    private Guid? _currentOrderId;

    public bool IsManager => Roles.Any(r => string.Equals(r, "Manager", StringComparison.OrdinalIgnoreCase) || string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
}
