namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// Resolves the path to the <c>adb</c> executable. Resolution happens live on each access so a Settings
/// override applies without a restart.
/// </summary>
public interface IAdbLocator
{
    /// <summary>
    /// The resolved <c>adb</c> path: the Settings override, else <c>ANDROID_HOME</c>/<c>ANDROID_SDK_ROOT</c>'s
    /// <c>platform-tools</c>, else the platform-default SDK location, else <c>adb</c> on <c>PATH</c>. Falls back
    /// to the bare <c>adb</c> name (resolved by the OS at launch) when nothing else is found.
    /// </summary>
    string AdbPath { get; }

    /// <summary>
    /// <see langword="true"/> when <c>adb</c> resolves to a real file (override, env var, default SDK, or
    /// <c>PATH</c>); <see langword="false"/> when only the bare-name fallback remains. Evaluated live.
    /// </summary>
    bool IsAvailable { get; }
}
