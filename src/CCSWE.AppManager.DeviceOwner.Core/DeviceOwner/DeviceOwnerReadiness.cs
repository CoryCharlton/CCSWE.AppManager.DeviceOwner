namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// The result of the device-owner pre-flight: whether App Manager is <see cref="AlreadyDeviceOwner"/> (a terminal
/// success) and, otherwise, the <see cref="Blockers"/> that would make <c>set-device-owner</c> fail.
/// </summary>
public sealed record DeviceOwnerReadiness(bool AlreadyDeviceOwner, IReadOnlyList<PreflightBlocker> Blockers)
{
    /// <summary>No blockers and not already owned — safe to run <c>set-device-owner</c>.</summary>
    public bool IsReady => !AlreadyDeviceOwner && Blockers.Count == 0;
}
