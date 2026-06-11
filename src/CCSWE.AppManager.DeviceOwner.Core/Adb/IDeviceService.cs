namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// Lists the devices the local adb server currently sees (<c>adb devices -l</c>).
/// </summary>
public interface IDeviceService
{
    /// <summary>Returns the currently attached devices, in adb's reported order.</summary>
    Task<IReadOnlyList<AdbDevice>> ListAsync(CancellationToken cancellationToken = default);
}
