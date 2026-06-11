using CCSWE.AppManager.DeviceOwner.Core.Settings;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <inheritdoc />
public sealed class AdbLocator : IAdbLocator
{
    private readonly IExecutableFinder _executableFinder;
    private readonly ISettingsService _settings;

    public AdbLocator(ISettingsService settings, IExecutableFinder executableFinder, ILogger<AdbLocator> logger)
    {
        _settings = settings;
        _executableFinder = executableFinder;

        if (Resolve() is null)
        {
            logger.LogWarning("adb not found via the Settings override, ANDROID_HOME/ANDROID_SDK_ROOT, the default SDK location, or PATH; falling back to the bare 'adb' name.");
        }
    }

    /// <inheritdoc />
    public string AdbPath => Resolve() ?? Executable("adb");

    /// <inheritdoc />
    public bool IsAvailable => Resolve() is not null;

    private static string DefaultSdkRoot()
    {
        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "Android", "Sdk");
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(home, "Library", "Android", "sdk");
        }

        return Path.Combine(home, "Android", "Sdk");
    }

    private static string Executable(string name) => OperatingSystem.IsWindows() ? $"{name}.exe" : name;

    // Override → ANDROID_HOME → ANDROID_SDK_ROOT (deprecated) → platform-default SDK → PATH; the first that
    // points at a real adb. Null means none matched (caller falls back to the bare name).
    private string? Resolve()
    {
        var adb = Executable("adb");

        var overridePath = _settings.AdbPath;
        if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
        {
            return overridePath;
        }

        foreach (var variable in new[] { "ANDROID_HOME", "ANDROID_SDK_ROOT" })
        {
            var root = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(root))
            {
                var candidate = Path.Combine(root, "platform-tools", adb);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        var defaultCandidate = Path.Combine(DefaultSdkRoot(), "platform-tools", adb);
        if (File.Exists(defaultCandidate))
        {
            return defaultCandidate;
        }

        return _executableFinder.FindOnPath("adb");
    }
}
