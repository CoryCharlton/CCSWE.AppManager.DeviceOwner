using CCSWE.AppManager.DeviceOwner.Core.Common;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <inheritdoc />
public sealed class DeviceService : IDeviceService
{
    private readonly IAdbLocator _adbLocator;
    private readonly IDeviceDetailsResolver _detailsResolver;
    private readonly ILogger<DeviceService> _logger;
    private readonly IProcessRunner _processRunner;

    public DeviceService(IProcessRunner processRunner, IAdbLocator adbLocator, IDeviceDetailsResolver detailsResolver, ILogger<DeviceService> logger)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
        _detailsResolver = detailsResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdbDevice>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = await _processRunner.RunAsync(_adbLocator.AdbPath, ["devices", "-l"], cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("adb devices -l exited with code {ExitCode}: {Error}", result.ExitCode, result.StandardError);
        }

        var devices = AdbOutputParser.ParseDeviceList(result.StandardOutput);

        // Enrich online devices with friendly details (cached by serial, so this is one getprop per device once).
        return await Task.WhenAll(devices.Select(device => EnrichAsync(device, cancellationToken)));
    }

    private async Task<AdbDevice> EnrichAsync(AdbDevice device, CancellationToken cancellationToken)
    {
        if (!device.IsOnline)
        {
            return device;
        }

        var details = await _detailsResolver.ResolveAsync(device.Serial, cancellationToken);
        return details is null
            ? device
            : device with { Name = details.Name, Form = details.Form, IsEmulator = details.IsEmulator };
    }
}
