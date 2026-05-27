using Microsoft.Extensions.Hosting;
using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.History;

// Periodic background sweep that prunes messages older than the configured
// retention window. Runs once at startup, then every _interval.
public sealed class HistoryRetentionService(HistoryRepository history, AppSettings settings) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First sweep right away so stale rows are cleaned on launch.
        Sweep();

        var timer = new PeriodicTimer(_interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                Sweep();
        }
        catch (OperationCanceledException) { /* shutdown */ }
    }

    private void Sweep()
    {
        try
        {
            history.DeleteOlderThan(settings.HistoryRetentionDays);
        }
        catch { /* sweep failure is non-fatal */ }
    }
}
