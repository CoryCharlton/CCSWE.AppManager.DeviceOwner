namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// A device reported by <c>adb devices -l</c>. <see cref="State"/> is the adb connection state
/// (<c>device</c>, <c>offline</c>, <c>unauthorized</c>, …); the descriptive columns are only present for
/// devices in the online <c>device</c> state.
/// </summary>
public sealed record AdbDevice(string Serial, string State, string? Model, string? Product, string? Device, string? TransportId)
{
    /// <summary>The device's form factor, resolved from properties (online devices only); defaults to a phone.</summary>
    public DeviceForm Form { get; init; } = DeviceForm.Phone;

    /// <summary>Whether this is an emulator, resolved from properties (online devices only).</summary>
    public bool IsEmulator { get; init; }

    /// <summary>A friendly device name resolved from device properties, when available (online devices only).</summary>
    public string? Name { get; init; }

    /// <summary>Whether the device is online and usable (adb state <c>device</c>).</summary>
    public bool IsOnline => State.Equals("device", StringComparison.Ordinal);
}
