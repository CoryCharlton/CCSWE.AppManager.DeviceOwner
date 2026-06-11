using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using CCSWE.AppManager.DeviceOwner.Desktop.Shell;
using CCSWE.AppManager.DeviceOwner.Desktop.Theming;
using CCSWE.Avalonia.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CCSWE.AppManager.DeviceOwner.Desktop;

[ExcludeFromCodeCoverage]
public partial class App : Application, IServiceProviderAccessor
{
    /// <summary>The host's service provider, set by the host before framework initialization completes. Null at
    /// design time (the previewer constructs <see cref="App"/> without a host), so composition is skipped then.</summary>
    public IServiceProvider? Services { get; set; }

    /// <summary>Receives the built provider from <see cref="IServiceProviderAccessor"/> (the host's injection seam).</summary>
    IServiceProvider IServiceProviderAccessor.Services
    {
        set => Services = value;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Services is null)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        // Apply the persisted theme and density now that Avalonia's styles are loaded.
        var settings = Services.GetRequiredService<ISettingsService>();
        Services.GetRequiredService<IThemeApplier>().Apply(settings.Theme);
        Services.GetRequiredService<IDensityApplier>().Apply(settings.Density);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
