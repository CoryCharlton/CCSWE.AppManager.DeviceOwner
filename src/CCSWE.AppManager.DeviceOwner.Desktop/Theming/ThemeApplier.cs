using Avalonia;
using Avalonia.Styling;
using CCSWE.AppManager.DeviceOwner.Core.Settings;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Theming;

/// <inheritdoc cref="IThemeApplier"/>
public sealed class ThemeApplier : IThemeApplier
{
    public void Apply(AppTheme theme)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = ToVariant(theme);
        }
    }

    private static ThemeVariant ToVariant(AppTheme theme) => theme switch
    {
        AppTheme.Light => ThemeVariant.Light,
        AppTheme.Dark => ThemeVariant.Dark,
        _ => ThemeVariant.Default,
    };
}
