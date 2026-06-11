using Avalonia.Threading;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;

/// <inheritdoc cref="IDispatcherTimer" />
public sealed class DispatcherTimerAdapter : IDispatcherTimer
{
    private readonly DispatcherTimer _timer;

    public DispatcherTimerAdapter(TimeSpan interval)
    {
        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += (sender, _) => Tick?.Invoke(sender, EventArgs.Empty);
    }

    /// <inheritdoc />
    public event EventHandler? Tick;

    /// <inheritdoc />
    public void Start() => _timer.Start();

    /// <inheritdoc />
    public void Stop() => _timer.Stop();
}
