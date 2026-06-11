namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// An owner entry parsed from <c>dpm list-owners</c>: the admin <see cref="Component"/> (and its
/// <see cref="Package"/>) plus whether it holds the device-owner or profile-owner role on <see cref="UserId"/>.
/// </summary>
public sealed record AdbOwner(int? UserId, string Component, string Package, bool IsDeviceOwner, bool IsProfileOwner);
