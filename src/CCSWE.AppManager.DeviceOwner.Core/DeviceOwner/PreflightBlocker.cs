namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>Why a device isn't ready for <c>set-device-owner</c>.</summary>
public enum PreflightBlockerKind
{
    AppNotInstalled,
    DeviceOwnerByOther,
    ProfileOwner,
    MultipleUsers,
    AccountsPresent,
    NotConnected,
}

/// <summary>A single reason the device isn't ready, with the user-facing <see cref="Message"/>.</summary>
public sealed record PreflightBlocker(PreflightBlockerKind Kind, string Message);
