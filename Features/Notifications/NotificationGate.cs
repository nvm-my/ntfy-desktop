using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Notifications;

// Owns the notification-pause read/write surface. Writes through to
// AppSettings.IsPaused and TopicSettings.IsPaused — same persistence format as
// before, just a single owner instead of scattered settings reads/writes.
//
// The pause state was previously bolted onto ConnectionManager, which conflated
// "is the socket healthy" with "are we delivering toasts". They're independent:
// sockets stay open while paused, messages still arrive and are persisted, only
// the toast is suppressed (see ShowToastNotification).
public sealed class NotificationGate
{
    private readonly AppSettings _settings;

    public NotificationGate(AppSettings settings)
    {
        _settings = settings;
    }

    public NotificationStatus GlobalStatus =>
        _settings.IsPaused ? NotificationStatus.Paused : NotificationStatus.Active;

    public bool IsGloballyPaused => _settings.IsPaused;

    public bool IsTopicPaused(string topicName)
    {
        if (_settings.IsPaused) return true;
        return _settings.GetTopicSettings(topicName)?.IsPaused ?? false;
    }

    // True only when the per-topic flag is set (ignores global pause).
    // Used by UI surfaces that need to distinguish "topic-specific pause" from
    // "everything paused".
    public bool IsTopicSpecificallyPaused(string topicName) =>
        _settings.GetTopicSettings(topicName)?.IsPaused ?? false;

    // Fires when the global pause flag flips.
    public event EventHandler? GlobalStatusChanged;

    // Fires when a single topic's pause flag flips. Arg is the topic name.
    public event EventHandler<string>? TopicPauseChanged;

    public void PauseAll()
    {
        if (_settings.IsPaused) return;
        _settings.IsPaused = true;
        _settings.Save();
        GlobalStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResumeAll()
    {
        if (!_settings.IsPaused) return;
        _settings.IsPaused = false;
        _settings.Save();
        GlobalStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public void PauseTopic(string topicName)
    {
        var t = _settings.GetTopicSettings(topicName);
        if (t is null || t.IsPaused) return;
        t.IsPaused = true;
        _settings.Save();
        TopicPauseChanged?.Invoke(this, topicName);
    }

    public void ResumeTopic(string topicName)
    {
        var t = _settings.GetTopicSettings(topicName);
        if (t is null || !t.IsPaused) return;
        t.IsPaused = false;
        _settings.Save();
        TopicPauseChanged?.Invoke(this, topicName);
    }
}
