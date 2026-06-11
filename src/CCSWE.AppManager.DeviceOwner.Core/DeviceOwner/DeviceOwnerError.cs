namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// Maps the raw <c>dpm set-device-owner</c> output onto the friendly <see cref="DeviceOwnerMessages"/> guidance,
/// so the desktop and console show actionable text instead of a Java stack trace. Matching is on substrings (the
/// <c>IllegalStateException</c> prefix and trailing period vary by Android version). Mirrors
/// <c>PlatformTools/DownloadError</c>.
/// </summary>
public static class DeviceOwnerError
{
    public static string Describe(string? output)
    {
        var text = output?.Trim() ?? string.Empty;

        if (Has(text, "already several users"))
        {
            return DeviceOwnerMessages.Users;
        }

        if (Has(text, "already some accounts"))
        {
            return DeviceOwnerMessages.AccountsPresent;
        }

        if (Has(text, "already provisioned") || Has(text, "already set-up") || Has(text, "already set up"))
        {
            return DeviceOwnerMessages.AlreadyProvisioned;
        }

        if (Has(text, "device owner is already set") || Has(text, "device owner is already configured"))
        {
            return DeviceOwnerMessages.DeviceOwnerByOther;
        }

        if (Has(text, "Unknown admin") || Has(text, "Bad admin"))
        {
            return DeviceOwnerMessages.AppNotInstalled;
        }

        if (Has(text, "offline") || Has(text, "unauthorized") || Has(text, "not found"))
        {
            return DeviceOwnerMessages.NotConnected;
        }

        return text.Length == 0 ? "adb reported a failure without any details." : text;
    }

    private static bool Has(string text, string token) => text.Contains(token, StringComparison.OrdinalIgnoreCase);
}
