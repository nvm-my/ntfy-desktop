using NtfyDesktop.Domain;
using NtfyDesktop.Features.Rules.Ai;
using NtfyDesktop.Features.Rules.Model;

namespace NtfyDesktop.Tests.Rules;

public class PackSummarizerTests
{
    [Fact]
    public void Summarize_CoversEachRuleType()
    {
        var pack = new RulePack("P",
            [new MatchRule(new Matcher { TitleRegex = "succeeded" }, [new RuleAction(RuleActionKind.SuppressToast)])],
            [new CorrelateRule("P#1", new Matcher { TitleRegex = "^PROBLEM" }, new Matcher { TitleRegex = "^RESOLVED" },
                new KeySelector { From = KeyField.Title, Regex = "#(?<key>\\d+)" })],
            [new ExpectRule("P#2", new Matcher { Topic = "backups" }, TimeSpan.FromHours(26), TimeSpan.FromHours(1),
                new AlertSpec(Priority.Urgent, "missed", null), null)]);

        var lines = PackSummarizer.Summarize(pack);
        Assert.Equal(3, lines.Count);
        Assert.Contains(lines, l => l.Contains("Suppress") && l.Contains("succeeded"));
        Assert.Contains(lines, l => l.Contains("Pair") || l.Contains("Correlate"));
        Assert.Contains(lines, l => l.Contains("Alert") && l.Contains("26"));
    }
}
