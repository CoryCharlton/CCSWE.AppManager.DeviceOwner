namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// Resolves a device's friendly <see cref="DeviceDetails"/> from its properties, cached by serial so the
/// device-list poll doesn't re-query every refresh.
/// </summary>
public interface IDeviceDetailsResolver
{
    /// <summary>Returns the device's details, or <see langword="null"/> if they can't be read.</summary>
    Task<DeviceDetails?> ResolveAsync(string serial, CancellationToken cancellationToken = default);
}
