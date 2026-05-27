using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.Topics;

public class TopicSettings
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool IsPaused { get; set; } = false;
    public Priority? MinPriority { get; set; } = null;

    // null = inherit global; true/false = override global
    public bool? ActiveHoursEnabled { get; set; } = null;
    public TimeOnly ActiveHoursStart { get; set; } = new TimeOnly(9, 0);
    public TimeOnly ActiveHoursEnd { get; set; } = new TimeOnly(18, 0);

    public TopicSettings Clone() => new()
    {
        Name = Name,
        Enabled = Enabled,
        IsPaused = IsPaused,
        MinPriority = MinPriority,
        ActiveHoursEnabled = ActiveHoursEnabled,
        ActiveHoursStart = ActiveHoursStart,
        ActiveHoursEnd = ActiveHoursEnd,
    };
}
