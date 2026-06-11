namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <inheritdoc />
public sealed class ConfirmDialog : IConfirmDialog
{
    /// <inheritdoc />
    public Task<bool> ConfirmAsync(string title, string message, string confirmLabel) =>
        DialogHost.ShowAsync<ConfirmDialogWindow>(new ConfirmDialogViewModel(title, message, confirmLabel));
}
