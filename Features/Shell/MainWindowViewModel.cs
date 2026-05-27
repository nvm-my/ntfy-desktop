using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NtfyDesktop.Features.Connections;
using NtfyDesktop.Features.Notifications;

namespace NtfyDesktop.Features.Shell;

// Owns the chrome state for MainWindow: connection-health pip + text in the
// title bar, and a separate "paused" chip when notifications are paused.
// Subscribes to both ConnectionManager (sockets) and NotificationGate (pause)
// because they're independent axes.
public sealed partial class MainWindowViewModel : ObservableObject
{
    private readonly ConnectionManager _connections;
    private readonly NotificationGate _gate;

    [ObservableProperty]
    private ConnectionStatus _connectionStatus;

    [ObservableProperty]
    private string _connectionStatusText = "Connecting…";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonLabel))]
    [NotifyPropertyChangedFor(nameof(ShowPauseButton))]
    private bool _isGloballyPaused;

    public string PauseButtonLabel => IsGloballyPaused ? "Resume notifications" : "Pause notifications";

    // When paused, the banner takes over as the resume entry point — the
    // title-bar button is hidden to avoid two redundant controls.
    public bool ShowPauseButton => !IsGloballyPaused;

    public MainWindowViewModel(ConnectionManager connections, NotificationGate gate)
    {
        _connections = connections;
        _gate = gate;
        _connections.ConnectionStatusChanged += OnConnectionChanged;
        _gate.GlobalStatusChanged += OnGateChanged;
        Refresh();
    }

    [RelayCommand]
    private void TogglePause()
    {
        if (_gate.IsGloballyPaused) _gate.ResumeAll();
        else                        _gate.PauseAll();
    }

    private void OnConnectionChanged(object? sender, EventArgs e) =>
        System.Windows.Application.Current?.Dispatcher.Invoke(Refresh);

    private void OnGateChanged(object? sender, EventArgs e) =>
        System.Windows.Application.Current?.Dispatcher.Invoke(Refresh);

    private void Refresh()
    {
        ConnectionStatus = _connections.GetConnectionStatus();
        ConnectionStatusText = ConnectionStatus switch
        {
            ConnectionStatus.Connected    => "Connected",
            ConnectionStatus.Degraded     => "Reconnecting…",
            ConnectionStatus.Disconnected => "Disconnected",
            _                             => "—",
        };
        IsGloballyPaused = _gate.IsGloballyPaused;
    }
}
