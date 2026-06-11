using CCSWE.AppManager.DeviceOwner.Core.Common;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <inheritdoc />
public sealed class DeviceService : IDeviceService
{
    private readonly IAdbLocator _adbLocator;
    private readonly ILogger<DeviceService> _logger;
    private readonly IProcessRunner _processRunner;

    public DeviceService(IProcessRunner processRunner, IAdbLocator adbLocator, ILogger<DeviceService> logger)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
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

        return AdbOutputParser.ParseDeviceList(result.StandardOutput);
    }
}
