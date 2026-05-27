using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using H.NotifyIcon;
using NtfyDesktop.Features.Connections;
using NtfyDesktop.Features.Notifications;

namespace NtfyDesktop.Features.Shell;

// Owns the system-tray icon. The "N" glyph is regenerated whenever the
// connection status changes so the user can see health at a glance without
// opening the window. Pause is independent: it doesn't affect the icon colour
// (sockets aren't unhealthy when paused). It does flip the menu item label
// and is reflected in the tooltip.
internal sealed class TrayIconHost : IDisposable
{
    // Same palette as the title-bar pip so the two surfaces agree.
    private static readonly Brush ConnectedBrush    = new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)); // green
    private static readonly Brush DegradedBrush     = new SolidColorBrush(Color.FromRgb(0xEA, 0x58, 0x0C)); // orange
    private static readonly Brush DisconnectedBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)); // red

    private readonly TaskbarIcon _icon;
    private readonly MenuItem _pauseItem;

    private ConnectionStatus _lastConnection = ConnectionStatus.Disconnected;
    private NotificationStatus _lastNotifications = NotificationStatus.Active;

    public TrayIconHost(App app)
    {
        _pauseItem = new MenuItem
        {
            Header = "Pause notifications",
            Command = new RelayCommand(app.TogglePause),
        };

        _icon = new TaskbarIcon
        {
            ToolTipText = App.NAME,
            ContextMenu = BuildContextMenu(app, _pauseItem),
            LeftClickCommand = new RelayCommand(app.ShowMainWindow),
            NoLeftClickDelay = true,
        };

        Render();
        _icon.ForceCreate();
    }

    public void SetConnectionStatus(ConnectionStatus status)
    {
        _lastConnection = status;
        Render();
    }

    public void SetNotificationStatus(NotificationStatus status)
    {
        _lastNotifications = status;
        Render();
    }

    private void Render()
    {
        var (brush, connectionWord) = _lastConnection switch
        {
            ConnectionStatus.Connected    => (ConnectedBrush,    "connected"),
            ConnectionStatus.Degraded     => (DegradedBrush,     "reconnecting"),
            ConnectionStatus.Disconnected => (DisconnectedBrush, "disconnected"),
            _                             => (DisconnectedBrush, "—"),
        };

        var generated = new GeneratedIconSource
        {
            Text = "N",
            Foreground = Brushes.White,
            Background = brush,
            FontFamily = new FontFamily("Segoe UI"),
            FontWeight = FontWeights.Bold,
        };

        _icon.Icon = generated.ToIcon();
        _icon.ToolTipText = _lastNotifications == NotificationStatus.Paused
            ? $"{App.NAME} — {connectionWord}, notifications paused"
            : $"{App.NAME} — {connectionWord}";

        _pauseItem.Header = _lastNotifications == NotificationStatus.Paused
            ? "Resume notifications"
            : "Pause notifications";
    }

    private static ContextMenu BuildContextMenu(App app, MenuItem pauseItem) =>
        new()
        {
            Items =
            {
                new MenuItem { Header = "Show", Command = new RelayCommand(app.ShowMainWindow) },
                new Separator(),
                pauseItem,
                new Separator(),
                new MenuItem { Header = "Disconnect all", Command = new RelayCommand(app.DisconnectAllConnections) },
                new MenuItem { Header = "Reconnect all",  Command = new RelayCommand(app.ReconnectAllConnections) },
                new Separator(),
                new MenuItem { Header = "Quit", Command = new RelayCommand(app.QuitApp) },
            },
        };

    public void Dispose() => _icon.Dispose();

    private sealed class RelayCommand(Action execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
        public event EventHandler? CanExecuteChanged
        {
            add { /* always executable */ }
            remove { /* always executable */ }
        }
    }
}
