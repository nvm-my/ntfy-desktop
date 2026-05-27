using CommunityToolkit.Mvvm.ComponentModel;
using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.Topics;

// Backs the add/edit-topic dialog. Operates on a clone so the user can cancel cleanly.
public sealed partial class TopicEditorViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _enabled = true;
    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _hasMinPriority;
    [ObservableProperty] private Priority _minPriority = Priority.Min;
    [ObservableProperty] private bool _overrideActiveHours;
    [ObservableProperty] private bool _activeHoursEnabled;
    [ObservableProperty] private TimeOnly _activeHoursStart = new(9, 0);
    [ObservableProperty] private TimeOnly _activeHoursEnd   = new(18, 0);

    public static TopicEditorViewModel FromTopic(TopicSettings? source)
    {
        if (source is null) return new TopicEditorViewModel();

        return new TopicEditorViewModel
        {
            Name = source.Name,
            Enabled = source.Enabled,
            IsPaused = source.IsPaused,
            HasMinPriority = source.MinPriority is not null,
            MinPriority = source.MinPriority ?? Priority.Min,
            OverrideActiveHours = source.ActiveHoursEnabled is not null,
            ActiveHoursEnabled = source.ActiveHoursEnabled ?? false,
            ActiveHoursStart = source.ActiveHoursStart,
            ActiveHoursEnd = source.ActiveHoursEnd,
        };
    }

    public TopicSettings ToTopic() => new()
    {
        Name = Name.Trim(),
        Enabled = Enabled,
        IsPaused = IsPaused,
        MinPriority = HasMinPriority ? MinPriority : null,
        ActiveHoursEnabled = OverrideActiveHours ? ActiveHoursEnabled : null,
        ActiveHoursStart = ActiveHoursStart,
        ActiveHoursEnd = ActiveHoursEnd,
    };
}
