using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using NtfyDesktop.Features.Connections;

namespace NtfyDesktop.Features.Shell.Converters;

// Green/amber/red pip used in the title bar to signal connection health.
// Pause is a separate concern — rendered as its own chip, not folded in here.
public sealed class ConnectionStatusToBrushConverter : IValueConverter
{
    private static readonly Brush _connected    = Frozen(Color.FromRgb(0x16, 0xA3, 0x4A));
    private static readonly Brush _degraded     = Frozen(Color.FromRgb(0xEA, 0x58, 0x0C));
    private static readonly Brush _disconnected = Frozen(Color.FromRgb(0xDC, 0x26, 0x26));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ConnectionStatus s
            ? s switch
            {
                ConnectionStatus.Connected    => _connected,
                ConnectionStatus.Degraded     => _degraded,
                ConnectionStatus.Disconnected => _disconnected,
                _                             => _disconnected,
            }
            : _disconnected;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static Brush Frozen(Color color)
    {
        var b = new SolidColorBrush(color);
        b.Freeze();
        return b;
    }
}
