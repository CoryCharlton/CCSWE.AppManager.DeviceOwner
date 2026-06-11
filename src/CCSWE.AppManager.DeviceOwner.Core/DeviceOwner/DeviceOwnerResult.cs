namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// The outcome of attempting to set the device owner. On failure <see cref="Message"/> carries the adb/dpm
/// output explaining why (e.g. accounts already on the device, app not installed).
/// </summary>
public sealed record DeviceOwnerResult(bool Success, string? Message)
{
    /// <summary>A successful result.</summary>
    public static DeviceOwnerResult Succeeded() => new(true, null);

    /// <summary>A failed result carrying the explanatory <paramref name="message"/>.</summary>
    public static DeviceOwnerResult Failed(string? message) => new(false, message);
}
