namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// Locates an executable on the <c>PATH</c> — used to find <c>adb</c> when it isn't under a resolved SDK root.
/// </summary>
public static class ExecutableLocator
{
    /// <summary>
    /// Returns the full path to <paramref name="fileName"/> found on <c>PATH</c> (trying the platform's
    /// executable extensions on Windows), or <see langword="null"/> if it isn't on <c>PATH</c>.
    /// </summary>
    public static string? FindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var names = ExecutableNames(fileName);

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var name in names)
            {
                var candidate = Path.Combine(directory, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ExecutableNames(string fileName)
    {
        // On non-Windows (and when the caller already gave an extension) the name is used as-is; otherwise try
        // the common Windows executable extensions.
        if (!OperatingSystem.IsWindows() || Path.HasExtension(fileName))
        {
            return [fileName];
        }

        return [fileName + ".exe", fileName + ".bat", fileName + ".cmd"];
    }
}
