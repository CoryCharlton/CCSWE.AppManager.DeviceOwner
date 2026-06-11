namespace CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;

/// <summary>Shows the modal download-progress dialog and reports whether platform tools were installed.</summary>
public interface IPlatformToolsInstallDialog
{
    /// <summary>Shows the dialog; returns <see langword="true"/> if adb was installed.</summary>
    Task<bool> ShowAsync();
}
