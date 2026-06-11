namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// Thrown when <c>adb</c> cannot be launched — typically because it is not installed or not on <c>PATH</c>
/// (and no Android SDK could be located).
/// </summary>
public sealed class ProcessLaunchException : Exception
{
    public ProcessLaunchException(string fileName, Exception innerException)
        : base($"Could not start '{fileName}'. Ensure adb is installed and on PATH (or set ANDROID_HOME for the Android SDK).", innerException)
    {
        FileName = fileName;
    }

    public string FileName { get; }
}
