namespace NtfyDesktop.Features.Settings;

/// <summary>
/// A configured ntfy server. Each server carries its own access token (DPAPI-encrypted
/// at rest), so topics on different servers authenticate independently.
/// </summary>
public sealed class ServerConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Friendly label shown in the UI (e.g. "Home", "ntfy.sh").</summary>
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = "https://ntfy.sh";

    public string EncryptedAccessToken { get; set; } = string.Empty;

    public string GetAccessToken() => TokenProtector.Decrypt(EncryptedAccessToken);

    public void SetAccessToken(string token) => EncryptedAccessToken = TokenProtector.Encrypt(token);

    /// <summary>Label for display: the friendly name if set, otherwise the host.</summary>
    public string DisplayLabel =>
        !string.IsNullOrWhiteSpace(Name)
            ? Name
            : (Uri.TryCreate(Url, UriKind.Absolute, out var u) ? u.Host : Url);
}
