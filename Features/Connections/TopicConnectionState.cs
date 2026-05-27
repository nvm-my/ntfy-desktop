namespace NtfyDesktop.Features.Connections;

// A snapshot of one topic's live socket state. Pure connection concerns:
// pause is a separate axis owned by Features.Notifications.NotificationGate and
// composed at the call site (Shell read models, Connections page rows).
public sealed record TopicConnectionState(
    string TopicName,
    TopicConnectionStatus Status,
    string? LastError);
