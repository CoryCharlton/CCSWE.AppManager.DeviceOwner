using CommunityToolkit.Mvvm.Input;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common;

/// <summary>View model for the reusable <see cref="MessageDialogWindow"/>: a title, a (possibly long) message,
/// and a single Close command that asks the host to dismiss.</summary>
public partial class MessageDialogViewModel : ViewModelBase, IDialogViewModel
{
    public MessageDialogViewModel(string title, string message)
    {
        Title = title;
        Message = message;
    }

    public event Action<bool>? CloseRequested;

    public string Message { get; }

    public string Title { get; }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke(true);
}
