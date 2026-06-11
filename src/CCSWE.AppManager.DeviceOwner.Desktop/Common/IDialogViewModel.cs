namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// A dialog view model that asks its host to close with a boolean result, so the view model never touches the
/// <c>Window</c>. Shown via <see cref="DialogHost"/>.
/// </summary>
public interface IDialogViewModel
{
    /// <summary>Raised when the dialog wants to close; the argument is the dialog's result.</summary>
    event Action<bool>? CloseRequested;
}
