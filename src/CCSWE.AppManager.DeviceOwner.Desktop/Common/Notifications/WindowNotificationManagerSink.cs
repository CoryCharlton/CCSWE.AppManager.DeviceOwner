using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

/// <summary>
/// An <see cref="INotificationSink"/> backed by an Avalonia <see cref="WindowNotificationManager"/>. Owns the
/// severity-to-<see cref="NotificationType"/> mapping and marshals onto the UI thread, keeping those concerns out
/// of <see cref="NotificationService"/>.
/// </summary>
public sealed class WindowNotificationManagerSink : INotificationSink
{
    private readonly WindowNotificationManager _manager;

    public WindowNotificationManagerSink(WindowNotificationManager manager)
    {
        _manager = manager;
    }

    /// <inheritdoc />
    public void Show(NotificationRequest request)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => Show(request));
            return;
        }

        _manager.Show(new Notification(
            request.Title,
            request.Message,
            ToNotificationType(request.Severity),
            request.Expiration,
            request.OnClick));
    }

    private static NotificationType ToNotificationType(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Success => NotificationType.Success,
        NotificationSeverity.Warning => NotificationType.Warning,
        NotificationSeverity.Error => NotificationType.Error,
        _ => NotificationType.Information,
    };
}
