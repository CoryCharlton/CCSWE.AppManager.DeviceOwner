namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <inheritdoc />
public sealed class MessageDialog : IMessageDialog
{
    /// <inheritdoc />
    public Task ShowAsync(string title, string message) =>
        DialogHost.ShowAsync<MessageDialogWindow>(new MessageDialogViewModel(title, message));
}
