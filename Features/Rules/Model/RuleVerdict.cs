namespace NtfyDesktop.Features.Rules.Model;

/// <summary>An incident to record as open (pending side-effect after the row is
/// confirmed new).</summary>
public sealed record IncidentOpen(string RuleId, string Key, string MessageId, long OpenedAt);

/// <summary>
/// The engine's decision for one message. <see cref="Suppress"/> drops the toast,
/// hides the row from the feed by default, and excludes it from the unread count.
/// <see cref="OpenIncident"/> / <see cref="CloseIncident"/> are incident-store
/// writes the caller applies only once the message is confirmed new.
/// </summary>
public sealed record RuleVerdict
{
    public bool Suppress { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IncidentOpen? OpenIncident { get; init; }
    public (string RuleId, string Key)? CloseIncident { get; init; }

    public static readonly RuleVerdict PassThrough = new();
}
