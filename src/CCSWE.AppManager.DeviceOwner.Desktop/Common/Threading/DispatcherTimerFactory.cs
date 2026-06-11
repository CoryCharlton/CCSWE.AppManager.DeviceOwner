namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;

/// <inheritdoc />
public sealed class DispatcherTimerFactory : ITimerFactory
{
    /// <inheritdoc />
    public IDispatcherTimer Create(TimeSpan interval) => new DispatcherTimerAdapter(interval);
}
