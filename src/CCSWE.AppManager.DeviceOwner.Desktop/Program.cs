using Avalonia;
using CCSWE.Avalonia.Hosting;

namespace CCSWE.AppManager.DeviceOwner.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code
    // before AppMain is called: things aren't initialized yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = DesktopApplication.CreateBuilder<App>(args, ConfigureOptions);
        builder.Services.AddDeviceOwnerDesktop();

#if DEBUG
        // Developer tools live here, not in the hosting library: they come from a Debug-only diagnostics package.
        builder.ConfigureAppBuilder(appBuilder => appBuilder.WithDeveloperTools());
#endif

        builder.Build().Run(args);
    }

    // Avalonia configuration, don't remove; also used by the visual designer. Mirrors the runtime AppBuilder
    // (minus the host) via the shared ConfigureOptions, so the previewer renders with the same platform/DPI setup.
    public static AppBuilder BuildAvaloniaApp() => DesktopApplication.ConfigureAppBuilder<App>(ConfigureOptions);

    private static void ConfigureOptions(DesktopApplicationOptions options) =>
        options.Win32PlatformOptions = new Win32PlatformOptions { DpiAwareness = Win32DpiAwareness.Unaware };
}
