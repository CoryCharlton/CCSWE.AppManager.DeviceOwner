namespace CCSWE.AppManager.DeviceOwner.Core.Settings;

/// <summary>
/// Reads and writes the persisted <see cref="SettingsModel"/> on disk. The read path is tolerant (a missing
/// or corrupt file yields defaults, never throws); the write path is atomic.
/// </summary>
public interface ISettingsStore
{
    /// <summary>Loads persisted settings; never throws — a missing or corrupt file yields defaults.</summary>
    SettingsModel Load();

    /// <summary>Persists settings atomically (temp file + move). Failures are logged, not thrown.</summary>
    void Save(SettingsModel settings);
}
