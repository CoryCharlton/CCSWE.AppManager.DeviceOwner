namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <summary>Maps the exceptions a platform-tools install can throw onto a friendly, actionable message, so the
/// desktop dialog and the console render the same guidance instead of a raw framework exception string.</summary>
public static class DownloadError
{
    public static string Describe(Exception exception) => exception switch
    {
        HttpRequestException { StatusCode: { } status } => $"The download server returned an error ({(int)status} {status}). Try again later.",
        HttpRequestException => "Couldn't reach the download server. Check your internet connection and try again.",
        TaskCanceledException or TimeoutException => "The download timed out. Check your internet connection and try again.",
        IOException or UnauthorizedAccessException => "Couldn't save the platform tools. Make sure there's enough free disk space and that the folder isn't in use, then try again.",

        // The installer's own messages (unsupported platform, malformed archive, adb missing after extraction)
        // are already user-facing.
        _ => exception.Message,
    };
}
