using System.Collections.Concurrent;
using CCSWE.AppManager.DeviceOwner.Core.Common;

namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <inheritdoc />
public sealed class DeviceDetailsResolver : IDeviceDetailsResolver
{
    private static readonly IReadOnlyList<string> Properties =
    [
        "ro.kernel.qemu",
        "ro.boot.qemu.avd_name",
        "ro.build.characteristics",
        "ro.product.model",
        "ro.product.marketing.name",
        "ro.product.vendor.marketname",
    ];

    private readonly IAdbLocator _adbLocator;
    private readonly ConcurrentDictionary<string, DeviceDetails> _cache = new(StringComparer.Ordinal);
    private readonly IProcessRunner _processRunner;

    public DeviceDetailsResolver(IProcessRunner processRunner, IAdbLocator adbLocator)
    {
        _processRunner = processRunner;
        _adbLocator = adbLocator;
    }

    /// <inheritdoc />
    public async Task<DeviceDetails?> ResolveAsync(string serial, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(serial, out var cached))
        {
            return cached;
        }

        // One shell round-trip reads every prop as `key=value` lines (values may contain spaces, never newlines).
        var script = string.Join("; ", Properties.Select(property => $"echo {property}=$(getprop {property})"));

        ProcessResult result;
        try
        {
            result = await _processRunner.RunAsync(_adbLocator.AdbPath, ["-s", serial, "shell", script], cancellationToken);
        }
        catch (ProcessLaunchException)
        {
            return null;
        }

        if (!result.Success)
        {
            return null;
        }

        var details = DeviceDetailsParser.Build(serial, result.StandardOutput);
        _cache[serial] = details;
        return details;
    }
}
