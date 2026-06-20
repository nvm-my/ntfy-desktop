namespace NtfyDesktop.Features.Rules.Model;

/// <summary>A straight pattern → actions rule.</summary>
public sealed record MatchRule(Matcher When, IReadOnlyList<RuleAction> Actions);

/// <summary>
/// Pairs an opening message with its resolving message via an extracted key.
/// A close message's actions only fire when a matching open incident exists.
/// <see cref="Id"/> namespaces incidents in the store (pack name + index).
/// </summary>
public sealed record CorrelateRule(
    string Id,
    Matcher Open,
    Matcher Close,
    KeySelector Key,
    IReadOnlyList<RuleAction> OnClose);

public sealed record RulePack(
    string Name,
    IReadOnlyList<MatchRule> MatchRules,
    IReadOnlyList<CorrelateRule> CorrelateRules);
