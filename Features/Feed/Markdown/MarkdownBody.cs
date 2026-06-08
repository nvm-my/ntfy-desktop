using System.Windows;
using System.Windows.Controls;

namespace NtfyDesktop.Features.Feed.Markdown;

/// <summary>
/// Attached properties that fill a <see cref="Panel"/> (the feed row's body host) with a
/// message body: a single wrapping <see cref="TextBlock"/> for plain text, or the rendered
/// block elements from <see cref="MarkdownRenderer"/> when <see cref="IsMarkdownProperty"/>
/// is set. Re-renders whenever either property changes — including when a virtualized row
/// recycles onto a new message (its DataContext changes and both bindings re-evaluate).
/// </summary>
public static class MarkdownBody
{
    private const string PrimaryTextBrush = "TextFillColorPrimaryBrush";

    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text", typeof(string), typeof(MarkdownBody),
        new PropertyMetadata(null, OnChanged));

    public static readonly DependencyProperty IsMarkdownProperty = DependencyProperty.RegisterAttached(
        "IsMarkdown", typeof(bool), typeof(MarkdownBody),
        new PropertyMetadata(false, OnChanged));

    public static void SetText(DependencyObject d, string? value) => d.SetValue(TextProperty, value);
    public static string? GetText(DependencyObject d) => (string?)d.GetValue(TextProperty);

    public static void SetIsMarkdown(DependencyObject d, bool value) => d.SetValue(IsMarkdownProperty, value);
    public static bool GetIsMarkdown(DependencyObject d) => (bool)d.GetValue(IsMarkdownProperty);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel) Render(panel);
    }

    private static void Render(Panel panel)
    {
        panel.Children.Clear();

        var text = GetText(panel);
        if (string.IsNullOrEmpty(text)) return;

        if (GetIsMarkdown(panel))
        {
            foreach (var element in MarkdownRenderer.Render(text))
                panel.Children.Add(element);
        }
        else
        {
            // Plain text — mirrors the previous single body TextBlock exactly.
            var tb = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap };
            tb.SetResourceReference(TextBlock.ForegroundProperty, PrimaryTextBrush);
            panel.Children.Add(tb);
        }
    }
}
