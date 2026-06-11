namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

/// <inheritdoc />
public sealed class NotificationService : INotificationService
{
    private readonly object _gate = new();
    private readonly List<NotificationRequest> _pending = [];
    private INotificationSink? _sink;

    /// <summary>
    /// Attaches (or, with <see langword="null"/>, clears) the sink that renders notifications — set by the shell
    /// window once it opens. Any notifications raised before the sink attached are flushed now, in order.
    /// </summary>
    public void SetSink(INotificationSink? sink)
    {
        List<NotificationRequest> buffered;

        lock (_gate)
        {
            _sink = sink;

            if (sink is null || _pending.Count == 0)
            {
                return;
            }

            buffered = [.. _pending];
            _pending.Clear();
        }

        foreach (var request in buffered)
        {
            sink.Show(request);
        }
    }

    /// <inheritdoc />
    public void Show(string title, string message, NotificationSeverity severity, TimeSpan? expiration = null, Action? onClick = null)
    {
        var request = new NotificationRequest(title, message, severity, expiration, onClick);

        INotificationSink? sink;

        lock (_gate)
        {
            if (_sink is null)
            {
                _pending.Add(request);
                return;
            }

            sink = _sink;
        }

        sink.Show(request);
    }
}
