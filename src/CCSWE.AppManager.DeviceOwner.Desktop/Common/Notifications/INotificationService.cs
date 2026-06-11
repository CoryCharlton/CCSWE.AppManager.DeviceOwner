namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

/// <summary>
/// Shows transient toast notifications over the main window. View models resolve this and call it; the shell
/// window owns the actual notification sink, so callers never need a <c>TopLevel</c>.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a notification. <paramref name="expiration"/> <see langword="null"/> uses the default timeout;
    /// <see cref="TimeSpan.Zero"/> keeps it until the user dismisses it. <paramref name="onClick"/> runs when the
    /// notification body is clicked.
    /// </summary>
    void Show(string title, string message, NotificationSeverity severity, TimeSpan? expiration = null, Action? onClick = null);
}
