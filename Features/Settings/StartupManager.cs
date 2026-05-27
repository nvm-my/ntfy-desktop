using Microsoft.Win32;

namespace NtfyDesktop.Features.Settings;

public static class StartupManager
{
    private const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string VALUE_NAME = App.NAME + "v" + App.MAJOR_VERSION;

    public static void Apply(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, writable: true);
            if (key == null) return;

            if (enable)
            {
                var exePath = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exePath)) return;
                key.SetValue(VALUE_NAME, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(VALUE_NAME, throwOnMissingValue: false);
            }
        }
        catch { /* registry access failure is non-fatal */ }
    }

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY);
            return key?.GetValue(VALUE_NAME) != null;
        }
        catch
        {
            return false;
        }
    }
}
