using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core;
using CCSWE.AppManager.DeviceOwner.Desktop.Common;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;
using CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;
using CCSWE.AppManager.DeviceOwner.Desktop.Shell;
using CCSWE.AppManager.DeviceOwner.Desktop.Theming;
using Microsoft.Extensions.DependencyInjection;

namespace CCSWE.AppManager.DeviceOwner.Desktop;

/// <summary>
/// Registers the Desktop head's services: the shared Core services, theming/density appliers, notifications,
/// and the main window and its view model.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeviceOwnerDesktop(this IServiceCollection services)
    {
        services.AddDeviceOwnerCore();

        services.AddSingleton<IThemeApplier, ThemeApplier>();
        services.AddSingleton<IDensityApplier, DensityApplier>();
        services.AddSingleton<ITimerFactory, DispatcherTimerFactory>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<INotificationService>(provider => provider.GetRequiredService<NotificationService>());

        services.AddSingleton<IConfirmDialog, ConfirmDialog>();
        services.AddSingleton<IPlatformToolsInstallDialog, PlatformToolsInstallDialog>();
        services.AddTransient<DownloadProgressDialogViewModel>();
        services.AddTransient<Func<DownloadProgressDialogViewModel>>(provider => provider.GetRequiredService<DownloadProgressDialogViewModel>);

        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}
