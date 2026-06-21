using System.Text;

namespace NtfyDesktop.Features.Rules.Ai;

/// <summary>
/// The app-owned prompt for drafting rule packs. The user never writes this — they
/// only provide sample messages and an optional one-line intent.
/// </summary>
public static class DraftPrompt
{
    public const string System = """
        You are an assistant that writes notification rule packs for the ntfy-desktop app.
        Output ONLY a single JSON object for one pack — no prose, no markdown fences.

        Pack shape:
        { "name": "<short name>", "rules": [ <rule>, ... ] }

        Rule types:
        - match:     { "type":"match", "when": <matcher>, "do":["suppressToast"|"tag:<text>"] }
                     Use to silence routine noise (no toast, hidden from the feed).
        - correlate: { "type":"correlate", "open": <matcher>, "close": <matcher>,
                       "key": { "from":"title"|"body", "regex":"...(?<key>...)..." } }
                     Pairs a problem with its resolution. BOTH must contain the SAME key
                     (a named group "key"); without a shared key, correlation cannot pair.
        - expect:    { "type":"expect", "when": <matcher>, "every":"26h", "grace":"1h",
                       "onAbsence": { "priority":"urgent", "title":"...", "message":"..." },
                       "onRecovery": { "priority":"default", "title":"..." } }
                     Use for "alert me if these messages STOP arriving". onRecovery optional.

        Matcher fields (all optional, ANDed): topic, minPriority (min|low|default|high|urgent),
        titleRegex, bodyRegex, tag. Regexes are case-insensitive; anchor with ^ / $.

        Base decisions only on the provided samples and the user's intent. Prefer specific
        regexes over broad ones. If unsure a correlation key exists, do not emit a correlate rule.
        """;

    public static IReadOnlyList<ChatMessage> BuildMessages(IReadOnlyList<string> samples, string? intent)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Sample messages:");
        foreach (var s in samples) sb.AppendLine($"- {s}");
        if (!string.IsNullOrWhiteSpace(intent))
        {
            sb.AppendLine();
            sb.AppendLine($"Intent: {intent.Trim()}");
        }
        sb.AppendLine();
        sb.AppendLine("Return the pack JSON now.");
        return [new ChatMessage("system", System), new ChatMessage("user", sb.ToString())];
    }
}
