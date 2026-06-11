namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// A device reported by <c>adb devices -l</c>. <see cref="State"/> is the adb connection state
/// (<c>device</c>, <c>offline</c>, <c>unauthorized</c>, …); the descriptive columns are only present for
/// devices in the online <c>device</c> state.
/// </summary>
public sealed record AdbDevice(string Serial, string State, string? Model, string? Product, string? Device, string? TransportId)
{
    /// <summary>Whether the device is online and usable (adb state <c>device</c>).</summary>
    public bool IsOnline => State.Equals("device", StringComparison.Ordinal);
}
