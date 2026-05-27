using System.IO;
using System.Windows;
using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NtfyDesktop.Features;
using NtfyDesktop.Features.Connections;
using NtfyDesktop.Features.Feed;
using NtfyDesktop.Features.Notifications;
using NtfyDesktop.Features.Shell;
using Wpf.Ui.Appearance;
using FeedViewModel = NtfyDesktop.Features.Feed.FeedViewModel;

namespace NtfyDesktop;

public partial class App : Application
{
    public const string NAME = "Ntfy Desktop";
    public const string MAJOR_VERSION = "3";
    private const string SINGLE_INSTANCE_MUTEX_NAME = NAME + "_v" + MAJOR_VERSION + "3_SingleInstance";

    public static readonly string DataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NtfyDesktop");

    private IHost? _host;
    private Mutex? _mutex;
    private TrayIconHost? _trayIcon;

    public static IServiceProvider Services =>
        ((App)Current)._host?.Services
            ?? throw new InvalidOperationException("Host not started yet.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Surface anything fatal during startup instead of silently dying.
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.ToString(), $"{NAME} — unhandled exception",
                MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            args.Handled = true;
            Shutdown(1);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            System.Windows.MessageBox.Show(args.ExceptionObject?.ToString() ?? "Unknown", $"{NAME} — fatal",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

        _mutex = new Mutex(true, SINGLE_INSTANCE_MUTEX_NAME, out var isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown(0);
            return;
        }

        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddNtfyDesktop();

        _host = builder.Build();

        _host.Services.UseMessaging();

        await _host.StartAsync();

        ApplicationThemeManager.ApplySystemTheme();

        // Pre-warm the feed VM so the SQLite backfill happens before the user
        // opens the window. By the time they click Show, Messages is populated.
        _ = _host.Services.GetRequiredService<FeedViewModel>();

        _trayIcon = new(this);

        // Tray reflects two independent axes — wire each.
        var conn = _host.Services.GetRequiredService<ConnectionManager>();
        var gate = _host.Services.GetRequiredService<NotificationGate>();

        _trayIcon.SetConnectionStatus(conn.GetConnectionStatus());
        _trayIcon.SetNotificationStatus(gate.GlobalStatus);

        conn.ConnectionStatusChanged += (_, _) =>
            Dispatcher.Invoke(() => _trayIcon?.SetConnectionStatus(conn.GetConnectionStatus()));
        gate.GlobalStatusChanged += (_, _) =>
            Dispatcher.Invoke(() => _trayIcon?.SetNotificationStatus(gate.GlobalStatus));
    }

    public void ShowMainWindow()
    {
        var window = _host!.Services.GetRequiredService<MainWindow>();

        if (!window.IsVisible)
            window.Show();

        if (window.WindowState == WindowState.Minimized)
            window.WindowState = WindowState.Normal;

        window.Activate();
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }

    public void TogglePause()
    {
        var gate = _host!.Services.GetRequiredService<NotificationGate>();
        if (gate.IsGloballyPaused) gate.ResumeAll();
        else                       gate.PauseAll();
    }

    public async void DisconnectAllConnections()
    {
        var conn = _host!.Services.GetRequiredService<ConnectionManager>();
        await conn.DisconnectAllAsync();
    }

    public async void ReconnectAllConnections()
    {
        // Hard reset: tears down all sockets and brings them back up. The plain
        // ApplySettingsAsync would no-op now that it's idempotent.
        var conn = _host!.Services.GetRequiredService<ConnectionManager>();
        await conn.RestartAllAsync();
    }

    public void QuitApp() => Shutdown(0);

    protected override async void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();

        if (_host != null)
        {
            try
            {
                await _host.StopAsync(TimeSpan.FromSeconds(3));
            }
            catch { /* shutdown best-effort */ }
            _host.Dispose();
        }

        _mutex?.ReleaseMutex();
        _mutex?.Dispose();

        base.OnExit(e);
    }
}
