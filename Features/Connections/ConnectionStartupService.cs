using Microsoft.Extensions.Hosting;

namespace NtfyDesktop.Features.Connections;

// Starts the ntfy WebSocket connections at app startup and tears them down at shutdown.
public sealed class ConnectionStartupService(ConnectionManager connections) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) =>
        connections.ApplySettingsAsync();

    public async Task StopAsync(CancellationToken cancellationToken) =>
        await connections.DisposeAsync();
}
