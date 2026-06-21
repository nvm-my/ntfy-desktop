namespace NtfyDesktop.Features.Rules.Ai;

/// <summary>Minimal chat-completion seam over an OpenAI-compatible endpoint.
/// Returns the assistant's raw text content.</summary>
public interface IChatClient
{
    Task<string> CompleteAsync(IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}
