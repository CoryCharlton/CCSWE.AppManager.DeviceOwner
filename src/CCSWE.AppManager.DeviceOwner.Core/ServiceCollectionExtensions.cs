using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace CCSWE.AppManager.DeviceOwner.Core;

/// <summary>
/// Registers the shared Core services so both front-ends wire up identically.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeviceOwnerCore(this IServiceCollection services)
    {
        services.AddSingleton<IAdbLocator, AdbLocator>();
        services.AddSingleton<IDeviceOwnerService, DeviceOwnerService>();
        services.AddSingleton<IDeviceService, DeviceService>();
        services.AddSingleton<IExecutableFinder, ExecutableFinder>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<ISettingsStore, SettingsStore>();
        services.AddSingleton<ISettingsService, SettingsService>();

        return services;
    }
}
