namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// Builds <see cref="DeviceDetails"/> from a device's <c>getprop</c> output. Pure (no I/O), so it's
/// unit-testable. An emulator is named after the AVD it runs (e.g. <c>Pixel_9</c> → "Pixel 9"); a physical device
/// prefers its marketing name, else its model. The form factor comes from <c>ro.build.characteristics</c>.
/// </summary>
internal static class DeviceDetailsParser
{
    public static DeviceDetails Build(string serial, string getpropOutput)
    {
        var properties = Parse(getpropOutput);

        var characteristics = properties.GetValueOrDefault("ro.build.characteristics", string.Empty);

        var isEmulator = properties.GetValueOrDefault("ro.kernel.qemu") == "1"
            || Has(characteristics, "emulator")
            || serial.StartsWith("emulator-", StringComparison.Ordinal);

        var form = Has(characteristics, "tablet") ? DeviceForm.Tablet
            : Has(characteristics, "watch") ? DeviceForm.Watch
            : Has(characteristics, "automotive") ? DeviceForm.Automotive
            : Has(characteristics, "tv") ? DeviceForm.Television
            : DeviceForm.Phone;

        var name = isEmulator
            ? Clean(FirstNonEmpty(properties, "ro.boot.qemu.avd_name", "ro.product.model"))
            : Clean(FirstNonEmpty(properties, "ro.product.marketing.name", "ro.product.vendor.marketname", "ro.product.model"));

        return new DeviceDetails(string.IsNullOrEmpty(name) ? null : name, form, isEmulator);
    }

    // adb/AVD names sanitize spaces to underscores ("Pixel_9" → "Pixel 9"); undo that.
    private static string? Clean(string? value) => value?.Replace('_', ' ');

    private static string? FirstNonEmpty(IReadOnlyDictionary<string, string> properties, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (properties.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool Has(string characteristics, string token) =>
        characteristics.Split(',').Any(part => part.Trim().Equals(token, StringComparison.OrdinalIgnoreCase));

    private static Dictionary<string, string> Parse(string output)
    {
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var rawLine in output.AsSpan().EnumerateLines())
        {
            var line = rawLine.Trim();
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            properties[line[..separator].ToString()] = line[(separator + 1)..].Trim().ToString();
        }

        return properties;
    }
}
