namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// Locates an executable on the <c>PATH</c>. A thin, mockable wrapper over <see cref="ExecutableLocator"/> so
/// <see cref="IAdbLocator"/> stays unit-testable.
/// </summary>
public interface IExecutableFinder
{
    /// <summary>
    /// Returns the full path to <paramref name="fileName"/> on <c>PATH</c>, or <see langword="null"/> if absent.
    /// </summary>
    string? FindOnPath(string fileName);
}
