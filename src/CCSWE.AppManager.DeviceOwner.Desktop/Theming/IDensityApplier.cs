using CCSWE.AppManager.DeviceOwner.Core.Settings;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Theming;

/// <summary>
/// Applies an <see cref="AppDensity"/> to the running Avalonia application. Confines the
/// <c>Application.Current</c> static to one place so view models stay testable.
/// </summary>
public interface IDensityApplier
{
    /// <summary>Applies <paramref name="density"/> to the Material theme's density style.</summary>
    void Apply(AppDensity density);
}
