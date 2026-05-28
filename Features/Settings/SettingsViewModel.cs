using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NtfyDesktop.Domain;
using NtfyDesktop.Features.Connections;

namespace NtfyDesktop.Features.Settings;

// Backs the Settings page. Holds a working copy of AppSettings; on Save commits
// to disk and reapplies connections *only* when ServerUrl or AccessToken changed.
// Tracks IsDirty by comparing the live VM state to a snapshot captured on Load,
// so manually reverting an edit clears the dirty flag.
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly ConnectionManager _connections;

    // Suppress IsDirty tracking while Load() is repopulating from AppSettings.
    private bool _loading;

    // Snapshot of all editable values at last Load(). IsDirty := current != snapshot.
    private FormSnapshot _snapshot = FormSnapshot.Empty;

    // Properties whose PropertyChanged event shouldn't trigger a dirty recompute:
    // either IsDirty itself, or computed/derived properties driven by [NotifyPropertyChangedFor].
    private static readonly HashSet<string> _nonDirtyProperties =
    [
        nameof(SettingsViewModel.IsDirty),
        nameof(ServerUrlError),
        nameof(HasServerUrlError),
        nameof(HasInsecureTokenWarning),
        nameof(HasStoredAccessToken),
        nameof(CanSave),
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServerUrlError))]
    [NotifyPropertyChangedFor(nameof(HasServerUrlError))]
    [NotifyPropertyChangedFor(nameof(HasInsecureTokenWarning))]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SettingsViewModel.SaveCommand))]
    private string _serverUrl = "https://ntfy.sh";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInsecureTokenWarning))]
    private string _accessToken = string.Empty;

    [ObservableProperty] private Priority _globalMinPriority = Priority.Min;
    [ObservableProperty] private int _historyRetentionDays = 30;
    [ObservableProperty] private bool _startWithWindows;
    [ObservableProperty] private bool _activeHoursEnabled;
    [ObservableProperty] private string _activeHoursStartText = "09:00";
    [ObservableProperty] private string _activeHoursEndText   = "18:00";

    [ObservableProperty] private bool _isDirty;

    public SettingsViewModel(AppSettings settings, ConnectionManager connections)
    {
        _settings = settings;
        _connections = connections;
        Load();
    }

    public void Load()
    {
        _loading = true;
        ServerUrl            = _settings.DefaultServer.Url;
        AccessToken          = _settings.DefaultServer.GetAccessToken();
        GlobalMinPriority    = _settings.GlobalMinPriority;
        HistoryRetentionDays = _settings.HistoryRetentionDays;
        StartWithWindows     = StartupManager.IsEnabled();
        ActiveHoursEnabled   = _settings.ActiveHoursEnabled;
        ActiveHoursStartText = _settings.ActiveHoursStart.ToString("HH:mm");
        ActiveHoursEndText   = _settings.ActiveHoursEnd.ToString("HH:mm");

        _snapshot = TakeSnapshot();
        _loading = false;
        IsDirty = false;
    }

    /// <summary>True if the settings file currently has a saved (encrypted) token.</summary>
    public bool HasStoredAccessToken => !string.IsNullOrEmpty(_settings.DefaultServer.EncryptedAccessToken);

    /// <summary>Null if ServerUrl is valid; an error message otherwise.</summary>
    public string? ServerUrlError
    {
        get
        {
            var s = ServerUrl?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s))                          return "Server URL is required.";
            if (!Uri.TryCreate(s, UriKind.Absolute, out var uri))      return "Not a valid URL.";
            if (uri.Scheme != "http" && uri.Scheme != "https")         return "URL must use http or https.";
            return null;
        }
    }

    public bool HasServerUrlError => ServerUrlError is not null;
    public bool CanSave            => !HasServerUrlError;

    /// <summary>The URL is plain http and a token is set: TopicConnection will NOT
    /// send the bearer header over cleartext, so warn the user.</summary>
    public bool HasInsecureTokenWarning =>
        !string.IsNullOrEmpty(AccessToken) &&
        (ServerUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ?? false);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (_loading) return;
        if (e.PropertyName is null) return;
        if (_nonDirtyProperties.Contains(e.PropertyName)) return;

        IsDirty = !TakeSnapshot().Equals(_snapshot);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var newUrl       = ServerUrl.Trim();
        var urlChanged   = !string.Equals(_snapshot.ServerUrl, newUrl, StringComparison.Ordinal);
        var tokenChanged = !string.Equals(_snapshot.AccessToken, AccessToken, StringComparison.Ordinal);

        _settings.DefaultServer.Url = newUrl;
        if (tokenChanged)
            _settings.DefaultServer.SetAccessToken(AccessToken);

        _settings.GlobalMinPriority    = GlobalMinPriority;
        _settings.HistoryRetentionDays = HistoryRetentionDays;
        _settings.ActiveHoursEnabled   = ActiveHoursEnabled;
        if (TimeOnly.TryParseExact((string?) ActiveHoursStartText, "HH:mm", out var start))
            _settings.ActiveHoursStart = start;
        if (TimeOnly.TryParseExact((string?) ActiveHoursEndText, "HH:mm", out var end))
            _settings.ActiveHoursEnd = end;

        _settings.Save();
        StartupManager.Apply(StartWithWindows);

        _snapshot = TakeSnapshot();
        IsDirty = false;

        // Only restart connections when ServerUrl or AccessToken actually changed.
        // Other settings are read on demand at message-arrival time.
        if (urlChanged || tokenChanged)
            await _connections.RestartAllAsync();
    }

    [RelayCommand]
    private void Discard() => Load();

    private FormSnapshot TakeSnapshot() => new(
        ServerUrl,
        AccessToken,
        GlobalMinPriority,
        HistoryRetentionDays,
        StartWithWindows,
        ActiveHoursEnabled,
        ActiveHoursStartText,
        ActiveHoursEndText);

    private readonly record struct FormSnapshot(
        string ServerUrl,
        string AccessToken,
        Priority GlobalMinPriority,
        int HistoryRetentionDays,
        bool StartWithWindows,
        bool ActiveHoursEnabled,
        string ActiveHoursStartText,
        string ActiveHoursEndText)
    {
        public static readonly FormSnapshot Empty = new(
            string.Empty, string.Empty, Priority.Min, 0, false, false, string.Empty, string.Empty);
    }
}
