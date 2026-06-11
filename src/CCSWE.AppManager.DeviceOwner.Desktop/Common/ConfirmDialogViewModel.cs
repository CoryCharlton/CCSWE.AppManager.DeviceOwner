using CommunityToolkit.Mvvm.Input;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>
/// View model for the reusable <see cref="ConfirmDialogWindow"/>: a title, message, and confirm-button label,
/// with Confirm/Cancel commands that ask the host to close with the result.
/// </summary>
public partial class ConfirmDialogViewModel : ViewModelBase, IDialogViewModel
{
    public ConfirmDialogViewModel(string title, string message, string confirmLabel)
    {
        Title = title;
        Message = message;
        ConfirmLabel = confirmLabel;
    }

    /// <summary>Raised when the dialog wants to close; the argument is whether the user confirmed.</summary>
    public event Action<bool>? CloseRequested;

    public string ConfirmLabel { get; }

    public string Message { get; }

    public string Title { get; }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);
}
