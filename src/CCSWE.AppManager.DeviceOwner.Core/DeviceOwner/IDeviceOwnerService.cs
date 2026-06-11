namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// Sets the App Manager app as the device owner on a connected device, via
/// <c>adb shell dpm set-device-owner</c>.
/// </summary>
public interface IDeviceOwnerService
{
    /// <summary>The fixed App Manager admin component <c>dpm set-device-owner</c> is pointed at.</summary>
    string Component { get; }

    /// <summary>
    /// Runs <c>adb -s &lt;serial&gt; shell dpm set-device-owner</c> for the App Manager component and reports
    /// whether it took. dpm can report failure with a zero exit code, so a successful exit whose output mentions
    /// an exception is still treated as a failure.
    /// </summary>
    Task<DeviceOwnerResult> SetAsync(string serial, CancellationToken cancellationToken = default);
}
