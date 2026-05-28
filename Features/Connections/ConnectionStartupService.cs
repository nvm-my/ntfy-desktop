using Microsoft.Extensions.Hosting;
using NtfyDesktop.Features.History;
using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Connections;

// Starts the ntfy WebSocket connections at app startup and tears them down at shutdown.
public sealed class ConnectionStartupService(
    ConnectionManager connections,
    HistoryRepository history,
    AppSettings settings) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // One-time migration: stamp topic_id onto history rows that predate
        // multi-server (matched by topic name, which was unique back then). No-ops
        // once rows are stamped.
        history.BackfillTopicIds(
            settings.Topics.Select(t => (t.Name, t.Id, t.ServerId)));

        return connections.ApplySettingsAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken) =>
        await connections.DisposeAsync();
}
