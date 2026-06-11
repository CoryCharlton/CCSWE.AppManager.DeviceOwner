using System.Globalization;

namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <summary>Shared human-readable formatting for download progress, used by the desktop dialog, the console
/// progress line, and tests so they render bytes/speed/ETA identically.</summary>
public static class DownloadFormat
{
    private static readonly string[] Units = ["B", "KB", "MB", "GB"];

    public static string Bytes(long value)
    {
        double size = value;
        var unit = 0;

        while (size >= 1024 && unit < Units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        var format = unit == 0 ? "0" : "0.0";
        return $"{size.ToString(format, CultureInfo.InvariantCulture)} {Units[unit]}";
    }

    public static string SpeedAndEta(double bytesPerSecond, TimeSpan? eta)
    {
        var speed = $"{Bytes((long)bytesPerSecond)}/s";
        return eta is null ? speed : $"{speed} · {Duration(eta.Value)} remaining";
    }

    private static string Duration(TimeSpan value)
    {
        var total = value < TimeSpan.Zero ? TimeSpan.Zero : value;
        return total.TotalHours >= 1
            ? $"{(int)total.TotalHours}:{total.Minutes:00}:{total.Seconds:00}"
            : $"{total.Minutes}:{total.Seconds:00}";
    }
}
