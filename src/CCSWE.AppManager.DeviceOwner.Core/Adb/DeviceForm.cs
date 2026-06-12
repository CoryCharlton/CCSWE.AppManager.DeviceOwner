namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>The device's form factor, derived from <c>ro.build.characteristics</c> — used to pick its icon.</summary>
public enum DeviceForm
{
    Phone,
    Tablet,
    Watch,
    Television,
    Automotive,
}
