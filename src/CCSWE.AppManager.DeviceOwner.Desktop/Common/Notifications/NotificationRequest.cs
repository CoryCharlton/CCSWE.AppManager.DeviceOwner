namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

/// <summary>A request to surface one notification — the unit buffered and forwarded by
/// <see cref="NotificationService"/> to an <see cref="INotificationSink"/>. Domain types only, no Avalonia.</summary>
public sealed record NotificationRequest(
    string Title,
    string Message,
    NotificationSeverity Severity,
    TimeSpan? Expiration,
    Action? OnClick);
