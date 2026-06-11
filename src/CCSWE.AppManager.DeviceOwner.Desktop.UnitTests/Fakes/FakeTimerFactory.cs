using CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;

namespace CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Fakes;

/// <summary>An <see cref="ITimerFactory"/> whose timer never fires on its own; tests drive ticks explicitly via
/// <see cref="FakeDispatcherTimer.Tick"/>.</summary>
public sealed class FakeTimerFactory : ITimerFactory
{
    public FakeDispatcherTimer Timer { get; } = new();

    public IDispatcherTimer Create(TimeSpan interval) => Timer;
}

public sealed class FakeDispatcherTimer : IDispatcherTimer
{
    public bool IsRunning { get; private set; }

    public event EventHandler? Tick;

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    public void FireTick() => Tick?.Invoke(this, EventArgs.Empty);
}
