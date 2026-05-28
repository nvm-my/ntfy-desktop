using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NtfyDesktop.Domain;
using NtfyDesktop.Features.Connections;
using NtfyDesktop.Features.Settings;

namespace NtfyDesktop.Features.Topics;

// Exposes the configured topic list for the Topics page.
// Mutations route through SettingsManager and trigger a ConnectionManager re-apply
// so subscriptions actually reflect the change.
public sealed partial class TopicsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly ConnectionManager _connections;

    public ObservableCollection<TopicSettings> Topics { get; } = new();

    [ObservableProperty]
    private TopicSettings? _selectedTopic;

    [ObservableProperty]
    private bool _isEmpty = true;

    public TopicsViewModel(AppSettings settings, ConnectionManager connections)
    {
        _settings = settings;
        _connections = connections;
        Topics.CollectionChanged += (_, _) => IsEmpty = Topics.Count == 0;
        ReloadFromSettings();
    }

    public void ReloadFromSettings()
    {
        Topics.Clear();
        foreach (var t in _settings.Topics)
            Topics.Add(t);
    }

    public async Task AddOrUpdateAsync(TopicSettings edited, TopicSettings? original)
    {
        if (original is not null)
        {
            // Editing: keep the original's stable identity (Id, server, display name).
            // The editor dialog only edits name/pause/priority/active-hours for now;
            // the server picker + display-name field arrive with the multi-server UI.
            edited.Id = original.Id;
            edited.ServerId = original.ServerId;
            edited.DisplayName = original.DisplayName;

            var idx = _settings.Topics.IndexOf(original);
            if (idx >= 0) _settings.Topics[idx] = edited;
        }
        else
        {
            // New topic: assign to the default server (the UI to pick a server lands
            // in the multi-server PR).
            if (edited.ServerId == Guid.Empty)
                edited.ServerId = _settings.DefaultServerId;
            _settings.Topics.Add(edited);
        }

        _settings.Save();
        ReloadFromSettings();
        await _connections.ApplySettingsAsync();
    }

    [RelayCommand]
    private async Task RemoveAsync(TopicSettings topic)
    {
        _settings.Topics.Remove(topic);
        _settings.Save();

        ReloadFromSettings();

        await _connections.ApplySettingsAsync();
    }

    // Flip a topic's Enabled flag and re-apply so the socket actually starts/stops.
    // Same persistence path as editing the topic via the dialog.
    public async Task ToggleEnabledAsync(TopicSettings topic)
    {
        topic.Enabled = !topic.Enabled;
        _settings.Save();
        ReloadFromSettings();
        await _connections.ApplySettingsAsync();
    }
}
