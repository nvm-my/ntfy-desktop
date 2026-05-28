using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Notifications;

// Owns the notification-pause read/write surface. Writes through to
// AppSettings.IsPaused (global) and TopicSettings.IsPaused (per topic).
//
// Per-topic pause is keyed by TopicId (topic names are no longer unique across
// servers). The pause axis is independent of the socket: sockets stay open while
// paused, messages still arrive and are persisted, only the toast is suppressed
// (see ShowToastNotification).
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

    public bool IsTopicPaused(Guid topicId)
    {
        if (_settings.IsPaused) return true;
        return _settings.GetTopicById(topicId)?.IsPaused ?? false;
    }

    // True only when the per-topic flag is set (ignores global pause).
    // Used by UI surfaces that need to distinguish "topic-specific pause" from
    // "everything paused".
    public bool IsTopicSpecificallyPaused(Guid topicId) =>
        _settings.GetTopicById(topicId)?.IsPaused ?? false;

    // Fires when the global pause flag flips.
    public event EventHandler? GlobalStatusChanged;

    // Fires when a single topic's pause flag flips. Arg is the topic id.
    public event EventHandler<Guid>? TopicPauseChanged;

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

    public void PauseTopic(Guid topicId)
    {
        var t = _settings.GetTopicById(topicId);
        if (t is null || t.IsPaused) return;
        t.IsPaused = true;
        _settings.Save();
        TopicPauseChanged?.Invoke(this, topicId);
    }

    public void ResumeTopic(Guid topicId)
    {
        var t = _settings.GetTopicById(topicId);
        if (t is null || !t.IsPaused) return;
        t.IsPaused = false;
        _settings.Save();
        TopicPauseChanged?.Invoke(this, topicId);
    }
}
