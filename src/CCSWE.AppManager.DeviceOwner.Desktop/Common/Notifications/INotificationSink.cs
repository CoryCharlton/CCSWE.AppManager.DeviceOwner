namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

/// <summary>
/// The UI sink that actually renders a notification. <see cref="NotificationService"/> writes to this abstraction
/// rather than to a <c>WindowNotificationManager</c>, so the service stays free of the Avalonia control; the
/// implementing adapter owns the control and any UI-thread marshaling.
/// </summary>
public interface INotificationSink
{
    /// <summary>Renders the notification described by <paramref name="request"/>.</summary>
    void Show(NotificationRequest request);
}
