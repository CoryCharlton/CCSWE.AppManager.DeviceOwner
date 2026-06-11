using JetBrains.Annotations;

namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <summary>Downloads Google's standalone Android SDK Platform Tools, extracts <c>adb</c> to a persistent
/// per-user location, and records it as the Settings <c>adb</c> override so it is reused on later launches.</summary>
[PublicAPI]
public interface IPlatformToolsInstaller
{
    /// <summary><see langword="true"/> on the platforms Google ships a platform-tools archive for
    /// (Windows, macOS, Linux).</summary>
    bool IsSupportedPlatform { get; }

    /// <summary>Ensures platform tools are installed, reporting download progress, and returns the resolved
    /// <c>adb</c> path. A short-circuit returns immediately when an installed copy already exists.</summary>
    Task<string> InstallAsync(IProgress<DownloadProgress>? progress, CancellationToken cancellationToken = default);
}
