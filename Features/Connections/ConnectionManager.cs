using FastEndpoints;
using Microsoft.Win32;
using NtfyDesktop.Domain;
using NtfyDesktop.Features.History;
using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Connections;

// Owns live WebSocket subscriptions per configured topic. Pure connection
// concerns only — pause (whether toasts are delivered) lives in
// Features.Notifications.NotificationGate. Consumers that need both axes
// compose them at the call site.
public sealed class ConnectionManager : IAsyncDisposable
{
    private readonly AppSettings _settings;
    private readonly HistoryRepository _history;
    private readonly Dictionary<string, TopicConnection> _connections = new();

    public event EventHandler? ConnectionStatusChanged;
    public event EventHandler? TopicsChanged;

    public ConnectionManager(AppSettings settings, HistoryRepository history)
    {
        _settings = settings;
        _history = history;

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    public ConnectionStatus GetConnectionStatus()
    {
        // No live sockets — either zero topics configured, or the user explicitly
        // tore everything down. Either way: nothing is connected.
        if (_connections.Count == 0)
            return ConnectionStatus.Disconnected;

        return _connections.Values.All(c => c.Status == TopicConnectionStatus.Connected)
            ? ConnectionStatus.Connected
            : ConnectionStatus.Degraded;
    }

    /// <summary>
    /// Per-topic connection snapshot. Sourced from configured topics so the UI
    /// can show topics that haven't connected yet (or whose connection failed
    /// and was removed); the live connection is looked up where available.
    /// Pause is a separate axis — query NotificationGate at the call site.
    /// </summary>
    public IReadOnlyList<TopicConnectionState> GetTopicStates() =>
        _settings.Topics
            .Select(topicSettings =>
            {
                _connections.TryGetValue(topicSettings.Name, out var conn);
                return new TopicConnectionState(
                    topicSettings.Name,
                    conn?.Status ?? TopicConnectionStatus.Disconnected,
                    conn?.LastError);
            })
            .ToList();

    /// <summary>
    /// Idempotent: brings the live connection set in line with the configured
    /// enabled topics. Removes connections for topics that disappeared / became
    /// disabled, adds connections for newly-enabled topics, leaves untouched
    /// connections alone. Use RestartAllAsync() when ServerUrl or AccessToken
    /// changed and existing sockets must reauthenticate.
    /// </summary>
    public async Task ApplySettingsAsync()
    {
        var desiredTopics = _settings.Topics
            .Where(t => t.Enabled)
            .Select(t => t.Name)
            .ToHashSet();

        // Stop and remove connections that are no longer wanted.
        foreach (var name in _connections.Keys.Except(desiredTopics).ToList())
            await RemoveConnectionAsync(name);

        // Add connections for newly-enabled topics; existing ones keep running.
        foreach (var name in desiredTopics.Except(_connections.Keys).ToList())
            AddConnection(name);

        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        TopicsChanged?.Invoke(this, EventArgs.Empty);
    }

    // Full teardown + fresh start. Called when ServerUrl or AccessToken changed
    // (existing sockets need to reauthenticate) and from the user-facing
    // "Reconnect all" action.
    public async Task RestartAllAsync()
    {
        foreach (var conn in _connections.Values)
            await conn.StopAsync();
        _connections.Clear();

        await ApplySettingsAsync();
    }

    public void ReconnectTopic(string topicName)
    {
        if (_connections.TryGetValue(topicName, out var conn))
            conn.ForceReconnect();
    }

    public void ReconnectAll()
    {
        foreach (var conn in _connections.Values)
            conn.ForceReconnect();
    }

    // Hard-reset: tears down every WebSocket subscription. The connections stay
    // down until ApplySettingsAsync() (or "Reconnect all" in the UI) brings them
    // back. Settings are not touched, so an app restart resumes normal subscription.
    public async Task DisconnectAllAsync()
    {
        foreach (var conn in _connections.Values)
            await conn.StopAsync();

        _connections.Clear();

        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        TopicsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AddConnection(string topicName)
    {
        var conn = new TopicConnection(
            topicName,
            () => _settings.ServerUrl,
            () => _settings.GetAccessToken());

        conn.MessageReceived += OnMessageReceived;
        conn.StateChanged += OnTopicConnectionStatusChanged;

        _connections[topicName] = conn;

        conn.Start();
    }

    private async Task RemoveConnectionAsync(string topicName)
    {
        if (!_connections.TryGetValue(topicName, out var conn)) return;

        conn.MessageReceived -= OnMessageReceived;
        conn.StateChanged -= OnTopicConnectionStatusChanged;

        await conn.StopAsync();

        _connections.Remove(topicName);
    }

    private void OnTopicConnectionStatusChanged(object? sender, TopicConnectionStatus status)
    {
        ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnMessageReceived(object? sender, NtfyMessage message)
    {
        _history.Insert(message);

        new NtfyMessageReceived(message).PublishAsync(Mode.WaitForNone);
    }

    // Resume the connections regardless of notification-pause state — sockets
    // and pause are independent now. Toast suppression happens downstream in
    // ShowToastNotification via NotificationGate.
    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
            ReconnectAll();
    }

    public async ValueTask DisposeAsync()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;

        foreach (var conn in _connections.Values)
            await conn.DisposeAsync();
    }
}
