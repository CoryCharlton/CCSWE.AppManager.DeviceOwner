namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// Shows a reusable modal confirmation dialog, keeping view models free of window handling.
/// </summary>
public interface IConfirmDialog
{
    /// <summary>Shows the dialog; returns <see langword="true"/> if the user confirmed.</summary>
    Task<bool> ConfirmAsync(string title, string message, string confirmLabel);
}
