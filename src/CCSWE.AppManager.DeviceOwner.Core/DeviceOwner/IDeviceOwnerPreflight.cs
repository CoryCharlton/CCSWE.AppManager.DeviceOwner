namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// Runs read-only adb checks to determine whether a device is ready for <c>set-device-owner</c>, so the failure
/// reasons can be surfaced before running the command.
/// </summary>
public interface IDeviceOwnerPreflight
{
    /// <summary>Inspects the device and reports whether it's ready (or already owned by App Manager).</summary>
    Task<DeviceOwnerReadiness> CheckAsync(string serial, CancellationToken cancellationToken = default);
}
