using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

/// <inheritdoc />
public sealed class DeviceOwnerPreflight : IDeviceOwnerPreflight
{
    private readonly IAdbLocator _adbLocator;
    private readonly ILogger<DeviceOwnerPreflight> _logger;
    private readonly IProcessRunner _processRunner;

    public DeviceOwnerPreflight(IProcessRunner processRunner, IAdbLocator adbLocator, ILogger<DeviceOwnerPreflight> logger)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DeviceOwnerReadiness> CheckAsync(string serial, CancellationToken cancellationToken = default)
    {
        // list-owners first: an existing device owner is terminal — App Manager → success, anything else → blocked.
        var listOwners = await ShellAsync(serial, ["dpm", "list-owners"], cancellationToken);
        if (TryConnectionBlocker(listOwners, out var connectionBlocker))
        {
            return Blocked(connectionBlocker);
        }

        var owners = AdbOutputParser.ParseOwners(listOwners.StandardOutput);

        var deviceOwner = owners.FirstOrDefault(owner => owner.IsDeviceOwner);
        if (deviceOwner is not null)
        {
            return string.Equals(deviceOwner.Package, AppManagerAdmin.Package, StringComparison.OrdinalIgnoreCase)
                ? new DeviceOwnerReadiness(true, [])
                : Blocked(new PreflightBlocker(PreflightBlockerKind.DeviceOwnerByOther, DeviceOwnerMessages.DeviceOwnerByOther));
        }

        if (owners.Any(owner => owner.IsProfileOwner && owner.UserId is null or 0))
        {
            return Blocked(new PreflightBlocker(PreflightBlockerKind.ProfileOwner, DeviceOwnerMessages.ProfileOwner));
        }

        var pathTask = ShellAsync(serial, ["pm", "path", AppManagerAdmin.Package], cancellationToken);
        var usersTask = ShellAsync(serial, ["pm", "list", "users"], cancellationToken);
        var accountsTask = ShellAsync(serial, ["dumpsys", "account"], cancellationToken);
        await Task.WhenAll(pathTask, usersTask, accountsTask);

        var path = await pathTask;
        var users = await usersTask;
        var accounts = await accountsTask;

        foreach (var result in new[] { path, users, accounts })
        {
            if (TryConnectionBlocker(result, out var blocker))
            {
                return Blocked(blocker);
            }
        }

        var blockers = new List<PreflightBlocker>();

        if (!path.StandardOutput.Contains("package:", StringComparison.Ordinal))
        {
            blockers.Add(new PreflightBlocker(PreflightBlockerKind.AppNotInstalled, DeviceOwnerMessages.AppNotInstalled));
        }

        if (AdbOutputParser.ParseUserCount(users.StandardOutput) > 1)
        {
            blockers.Add(new PreflightBlocker(PreflightBlockerKind.MultipleUsers, DeviceOwnerMessages.Users));
        }

        if (AdbOutputParser.ParseAccountCount(accounts.StandardOutput) > 0)
        {
            blockers.Add(new PreflightBlocker(PreflightBlockerKind.AccountsPresent, DeviceOwnerMessages.AccountsPresent));
        }

        return new DeviceOwnerReadiness(false, blockers);
    }

    private static DeviceOwnerReadiness Blocked(PreflightBlocker blocker) => new(false, [blocker]);

    // adb transport failures land on stderr (e.g. "device offline", "device unauthorized.", "device '…' not
    // found"). Only stderr is inspected so normal command stdout can't trip a false positive.
    private static bool TryConnectionBlocker(ProcessResult result, out PreflightBlocker blocker)
    {
        var error = result.StandardError;

        if (error.Contains("offline", StringComparison.OrdinalIgnoreCase)
            || error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
            || error.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || error.Contains("no devices", StringComparison.OrdinalIgnoreCase)
            || error.Contains("still authorizing", StringComparison.OrdinalIgnoreCase))
        {
            blocker = new PreflightBlocker(PreflightBlockerKind.NotConnected, DeviceOwnerMessages.NotConnected);
            return true;
        }

        blocker = null!;
        return false;
    }

    private async Task<ProcessResult> ShellAsync(string serial, IReadOnlyList<string> shellArguments, CancellationToken cancellationToken)
    {
        var arguments = new List<string>(shellArguments.Count + 3) { "-s", serial, "shell" };
        arguments.AddRange(shellArguments);

        var result = await _processRunner.RunAsync(_adbLocator.AdbPath, arguments, cancellationToken);

        if (!result.Success)
        {
            _logger.LogDebug("adb {Arguments} exited {ExitCode}: {Error}", string.Join(' ', shellArguments), result.ExitCode, result.StandardError);
        }

        return result;
    }
}
