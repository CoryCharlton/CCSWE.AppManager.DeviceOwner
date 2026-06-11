using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

namespace CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Fakes;

/// <summary>An <see cref="INotificationService"/> that records the notifications it was asked to show.</summary>
public sealed class FakeNotificationService : INotificationService
{
    public List<(string Title, string Message, NotificationSeverity Severity)> Shown { get; } = [];

    public void Show(string title, string message, NotificationSeverity severity, TimeSpan? expiration = null, Action? onClick = null) =>
        Shown.Add((title, message, severity));
}
