using CCSWE.AppManager.DeviceOwner.Core.Common;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <inheritdoc />
public sealed class DeviceOwnerService : IDeviceOwnerService
{
    private readonly IAdbLocator _adbLocator;
    // Some devices (confirmed Samsung Galaxy S8) hang instead of reporting an error when accounts block activation;
    // bound the wait so the UI surfaces guidance rather than spinning forever.
    private readonly TimeSpan _commandTimeout;
    private readonly ILogger<DeviceOwnerService> _logger;
    private readonly IProcessRunner _processRunner;

    public DeviceOwnerService(IProcessRunner processRunner, IAdbLocator adbLocator, ILogger<DeviceOwnerService> logger, TimeSpan? commandTimeout = null)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
        _logger = logger;
        _commandTimeout = commandTimeout ?? TimeSpan.FromSeconds(60);
    }

    /// <inheritdoc />
    public string Component => AppManagerAdmin.Component;

    /// <inheritdoc />
    public async Task<DeviceOwnerResult> SetAsync(string serial, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serial))
        {
            return DeviceOwnerResult.Failed("A device must be selected.");
        }

        using var timeout = new CancellationTokenSource(_commandTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        ProcessResult result;
        try
        {
            result = await _processRunner.RunAsync(
                _adbLocator.AdbPath,
                ["-s", serial, "shell", "dpm", "set-device-owner", AppManagerAdmin.Component],
                linked.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("dpm set-device-owner timed out for {Serial}", serial);
            return DeviceOwnerResult.Failed(DeviceOwnerMessages.Timeout);
        }

        // dpm prints the real reason to stderr (or, on some platforms, stdout) and can exit 0 even when it
        // refused — e.g. "...because there are already some accounts on the device". So treat a non-zero exit OR
        // any "exception" in the output as a failure, mapping that message to friendly guidance for the caller.
        var message = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
        var mentionsException = !string.IsNullOrWhiteSpace(message)
            && message.Contains("exception", StringComparison.OrdinalIgnoreCase);

        if (!result.Success || mentionsException)
        {
            _logger.LogWarning("dpm set-device-owner failed for {Serial} (exit {ExitCode}): {Message}", serial, result.ExitCode, message);
            return DeviceOwnerResult.Failed(DeviceOwnerError.Describe(message));
        }

        return DeviceOwnerResult.Succeeded();
    }
}
