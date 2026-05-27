using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.Shell.Converters;

// Maps message priority to a colored accent brush, used for the leading
// stripe on inbox cards and small priority pips.
public sealed class PriorityToBrushConverter : IValueConverter
{
    private static readonly Dictionary<Priority, Brush> _brushes = new()
    {
        [Priority.Min]     = Frozen(Color.FromRgb(0x9C, 0xA3, 0xAF)),
        [Priority.Low]     = Frozen(Color.FromRgb(0x25, 0x63, 0xEB)),
        [Priority.Default] = Frozen(Color.FromRgb(0x6B, 0x72, 0x80)),
        [Priority.High]    = Frozen(Color.FromRgb(0xEA, 0x58, 0x0C)),
        [Priority.Urgent]  = Frozen(Color.FromRgb(0xDC, 0x26, 0x26)),
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Priority p && _brushes.TryGetValue(p, out var brush) ? brush : _brushes[Priority.Default];

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static Brush Frozen(Color color)
    {
        var b = new SolidColorBrush(color);
        b.Freeze();
        return b;
    }
}
