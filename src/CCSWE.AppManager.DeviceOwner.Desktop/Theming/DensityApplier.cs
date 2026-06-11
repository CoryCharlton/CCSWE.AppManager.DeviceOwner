using Avalonia;
using CCSWE.Avalonia.Material;
using CCSWE.AppManager.DeviceOwner.Core.Settings;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Theming;

/// <inheritdoc cref="IDensityApplier"/>
public sealed class DensityApplier : IDensityApplier
{
    public void Apply(AppDensity density)
    {
        // The Material theme lives in Application.Styles; flipping its DensityStyle re-resolves every
        // dimension DynamicResource live, with no restart.
        if (Application.Current?.Styles.OfType<MaterialTheme>().FirstOrDefault() is { } theme)
        {
            theme.DensityStyle = ToStyle(density);
        }
    }

    private static DensityStyle ToStyle(AppDensity density) => density switch
    {
        AppDensity.Compact => DensityStyle.Compact,
        _ => DensityStyle.Normal,
    };
}
