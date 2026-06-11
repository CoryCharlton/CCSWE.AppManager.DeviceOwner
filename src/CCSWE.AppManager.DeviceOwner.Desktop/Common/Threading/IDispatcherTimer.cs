namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;

/// <summary>
/// A repeating UI-thread timer abstraction over <c>Avalonia.Threading.DispatcherTimer</c>, so view models can
/// drive periodic work without depending on the Avalonia type (and can be unit-tested with a fake). Created via
/// <see cref="ITimerFactory"/>.
/// </summary>
public interface IDispatcherTimer
{
    /// <summary>Raised on each interval while the timer is running.</summary>
    event EventHandler Tick;

    /// <summary>Starts (or restarts) ticking at the configured interval.</summary>
    void Start();

    /// <summary>Stops ticking.</summary>
    void Stop();
}
