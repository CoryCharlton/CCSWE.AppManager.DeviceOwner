namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>Friendly details resolved from a device's properties: a display <see cref="Name"/>, its
/// <see cref="Form"/> factor, and whether it's an <see cref="IsEmulator"/>.</summary>
public sealed record DeviceDetails(string? Name, DeviceForm Form, bool IsEmulator);
