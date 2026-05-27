using System.Globalization;
using System.Windows.Data;

namespace NtfyDesktop.Features.Shell.Converters;

// "just now" / "5m ago" / "3h ago" / "yesterday" / absolute date for older entries.
public sealed class RelativeTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTimeOffset ts) return string.Empty;

        var delta = DateTimeOffset.Now - ts;

        if (delta.TotalSeconds < 60) return "just now";
        if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes}m ago";
        if (delta.TotalHours < 24)   return $"{(int)delta.TotalHours}h ago";
        if (delta.TotalDays < 2)     return "yesterday";
        if (delta.TotalDays < 7)     return $"{(int)delta.TotalDays}d ago";

        return ts.LocalDateTime.ToString("MMM d, yyyy", culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
