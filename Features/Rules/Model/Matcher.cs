using System.Text.RegularExpressions;
using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.Rules.Model;

/// <summary>
/// A predicate over a received message. All set fields are ANDed; an all-null
/// matcher matches every message. Regex fields are case-insensitive substring
/// searches (use ^/$ to anchor). Compiled regexes are cached per instance.
/// </summary>
public sealed record Matcher
{
    public string? Topic { get; init; }
    public Priority? MinPriority { get; init; }
    public string? TitleRegex { get; init; }
    public string? BodyRegex { get; init; }
    public string? Tag { get; init; }

    private Regex? _titleRe;
    private Regex? _bodyRe;

    public bool Matches(NtfyMessage message)
    {
        if (Topic is not null &&
            !string.Equals(Topic, message.Topic, StringComparison.OrdinalIgnoreCase))
            return false;

        if (MinPriority is { } min && message.Priority < min)
            return false;

        if (TitleRegex is not null)
        {
            _titleRe ??= Compile(TitleRegex);
            if (message.Title is null || !_titleRe.IsMatch(message.Title)) return false;
        }

        if (BodyRegex is not null)
        {
            _bodyRe ??= Compile(BodyRegex);
            if (message.Message is null || !_bodyRe.IsMatch(message.Message)) return false;
        }

        if (Tag is not null &&
            (message.Tags is null ||
             !message.Tags.Any(t => string.Equals(t, Tag, StringComparison.OrdinalIgnoreCase))))
            return false;

        return true;
    }

    private static Regex Compile(string pattern) =>
        new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
