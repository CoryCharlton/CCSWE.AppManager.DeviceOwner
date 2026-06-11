namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// Shows a modal message dialog with a single dismiss button — for long, actionable text (e.g. why
/// <c>set-device-owner</c> failed) that doesn't fit a transient toast.
/// </summary>
public interface IMessageDialog
{
    Task ShowAsync(string title, string message);
}
