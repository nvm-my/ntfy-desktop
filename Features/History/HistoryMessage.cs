using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.History;

public class HistoryMessage
{
    public long RowId { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public Priority Priority { get; set; } = Priority.Default;
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Tags { get; set; }
    public string? Click { get; set; }

    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? Topic : Title;
}
