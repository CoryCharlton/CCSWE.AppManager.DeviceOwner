namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;

/// <summary>Creates <see cref="IDispatcherTimer"/> instances, so consumers can stay free of the Avalonia timer type.</summary>
public interface ITimerFactory
{
    /// <summary>Creates a stopped timer that ticks every <paramref name="interval"/> once started.</summary>
    IDispatcherTimer Create(TimeSpan interval);
}
