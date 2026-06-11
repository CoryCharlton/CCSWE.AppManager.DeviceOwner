namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <summary>
/// App Manager's fixed device-admin identity. This helper exists solely to make App Manager the device owner, so
/// the package/component are constants rather than configurable, shared by the set and pre-flight services.
/// </summary>
internal static class AppManagerAdmin
{
    public const string Package = "com.ccswe.appmanager.deviceowner";

    public const string Component = $"{Package}/com.ccswe.appmanager.receivers.DeviceAdminReceiver";
}
