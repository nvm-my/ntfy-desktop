using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NtfyDesktop.Domain;
using NtfyDesktop.Features.Topics;

namespace NtfyDesktop.Features.Settings;

public class AppSettings
{
    private static readonly string _path = Path.Combine(App.DataPath, "settings.json");

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };
    
    public static AppSettings Load()
    {
        if (!File.Exists(_path)) return new();

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new();
        }
    }
    
    public void Save()
    {
        Directory.CreateDirectory(App.DataPath);
        var json = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(_path, json);
    }
    
    public string GetAccessToken()
    {
        if (string.IsNullOrEmpty(EncryptedAccessToken))
            return string.Empty;

        try
        {
            var encrypted = Convert.FromBase64String(EncryptedAccessToken);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }

    public void SetAccessToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            EncryptedAccessToken = string.Empty;
            return;
        }

        var data = Encoding.UTF8.GetBytes(token);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        EncryptedAccessToken = Convert.ToBase64String(encrypted);
    }

    #region props

    public string ServerUrl { get; set; } = "https://ntfy.sh";
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public Priority GlobalMinPriority { get; set; } = Priority.Min;
    public int HistoryRetentionDays { get; set; } = 30;
    // "Start with Windows" is stored exclusively in the HKCU\...\Run registry key
    // (see StartupManager); it has no representation in this file.
    public bool IsPaused { get; set; } = false;
    public bool ActiveHoursEnabled { get; set; } = false;
    public TimeOnly ActiveHoursStart { get; set; } = new TimeOnly(9, 0);
    public TimeOnly ActiveHoursEnd { get; set; } = new TimeOnly(18, 0);
    public List<TopicSettings> Topics { get; set; } = new();

    #endregion

    
    public TopicSettings? GetTopicSettings(string topicName)
        => Topics.FirstOrDefault(x => x.Name == topicName);
    
    
    
    
}
