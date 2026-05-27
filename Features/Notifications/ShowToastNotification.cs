using FastEndpoints;
using NtfyDesktop.Domain;
using NtfyDesktop.Features.Connections;
using NtfyDesktop.Features.Settings;
using NtfyDesktop.Features.Topics;

namespace NtfyDesktop.Features.Notifications;

/// <summary>
/// Displays a toast notification when an Ntfy message is received, if configuration allows it.
/// Pause checks go through NotificationGate so this class doesn't have to know
/// how pause is persisted.
/// </summary>
public class ShowToastNotification(
    AppSettings settings,
    NotificationGate gate,
    ToastNotifier toaster) : IEventHandler<NtfyMessageReceived>
{
    private ActiveHours ResolveActiveHours(TopicSettings? topicSettings)
    {
        return topicSettings is not null
            ? new(topicSettings.ActiveHoursEnabled ?? false, topicSettings.ActiveHoursStart, topicSettings.ActiveHoursEnd)
            : new(settings.ActiveHoursEnabled, settings.ActiveHoursStart, settings.ActiveHoursEnd);
    }

    private bool DropMessage(NtfyMessage message)
    {
        // drop if notifications are paused (globally or for this topic)
        if (gate.IsTopicPaused(message.Topic)) return true;

        var topicSettings = settings.GetTopicSettings(message.Topic);

        // drop if below min priority threshold
        var minPriority = topicSettings?.MinPriority ?? settings.GlobalMinPriority;
        if (message.Priority < minPriority) return true;

        // Resolve active hours (per-topic overrides global when non-null)
        var activeHours = ResolveActiveHours(topicSettings);

        // drop when active hours are enabled and current time is excluded
        if (activeHours.Enabled && activeHours.Excludes(TimeOnly.FromDateTime(DateTime.Now)))
            return true;

        return false;
    }

    public Task HandleAsync(NtfyMessageReceived eventModel, CancellationToken ct)
    {
        var message = eventModel.Message;

        if (!DropMessage(message)) toaster.Show(message);

        return Task.CompletedTask;
    }
}
