namespace NtfyDesktop.Features.Notifications;

// Whether the app is currently delivering toast notifications.
// Independent of connection health — sockets can be Connected while paused
// (messages still arrive and are persisted; only the toast is suppressed).
public enum NotificationStatus
{
    Active,
    Paused,
}
