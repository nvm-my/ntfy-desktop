using System.Security.Cryptography;
using System.Text;

namespace NtfyDesktop.Features.Settings;

/// <summary>
/// DPAPI (CurrentUser) helper for access tokens at rest. Round-trips a plaintext
/// token to/from a base64 string suitable for JSON storage. Shared by ServerConfig
/// (per-server tokens) and the legacy AppSettings token field.
/// </summary>
internal static class TokenProtector
{
    public static string Encrypt(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        var data = Encoding.UTF8.GetBytes(token);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public static string Decrypt(string encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64)) return string.Empty;
        try
        {
            var encrypted = Convert.FromBase64String(encryptedBase64);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            return string.Empty;
        }
    }
}
