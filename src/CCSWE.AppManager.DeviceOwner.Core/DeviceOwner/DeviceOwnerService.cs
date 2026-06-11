using CCSWE.AppManager.DeviceOwner.Core.Common;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <inheritdoc />
public sealed class DeviceOwnerService : IDeviceOwnerService
{
    // App Manager's DeviceAdminReceiver. This helper exists solely to make App Manager the device owner, so the
    // component is fixed rather than configurable.
    private const string AppManagerComponent = "com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver";

    private readonly IAdbLocator _adbLocator;
    private readonly ILogger<DeviceOwnerService> _logger;
    private readonly IProcessRunner _processRunner;

    public DeviceOwnerService(IProcessRunner processRunner, IAdbLocator adbLocator, ILogger<DeviceOwnerService> logger)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Component => AppManagerComponent;

    /// <inheritdoc />
    public async Task<DeviceOwnerResult> SetAsync(string serial, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serial))
        {
            return DeviceOwnerResult.Failed("A device must be selected.");
        }

        var result = await _processRunner.RunAsync(
            _adbLocator.AdbPath,
            ["-s", serial, "shell", "dpm", "set-device-owner", AppManagerComponent],
            cancellationToken);

        // dpm prints the real reason to stderr (or, on some platforms, stdout) and can exit 0 even when it
        // refused — e.g. "...because there are already some accounts on the device". So treat a non-zero exit OR
        // any "exception" in the output as a failure, carrying that message back to the caller.
        var message = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
        var mentionsException = !string.IsNullOrWhiteSpace(message)
            && message.Contains("exception", StringComparison.OrdinalIgnoreCase);

        if (!result.Success || mentionsException)
        {
            _logger.LogWarning("dpm set-device-owner failed for {Serial} (exit {ExitCode}): {Message}", serial, result.ExitCode, message);
            return DeviceOwnerResult.Failed(string.IsNullOrWhiteSpace(message) ? null : message.Trim());
        }

        return DeviceOwnerResult.Succeeded();
    }
}
