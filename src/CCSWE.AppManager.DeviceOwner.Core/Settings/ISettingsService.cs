namespace CCSWE.AppManager.DeviceOwner.Core.Settings;

/// <summary>
/// Stores user-configurable application settings. Shared by both front-ends so a setting changed in one
/// place is observed everywhere.
/// </summary>
public interface ISettingsService
{
    /// <summary>Override for the <c>adb</c> executable path, or <see langword="null"/> to use env-var/default/PATH resolution.</summary>
    string? AdbPath { get; set; }

    /// <summary>The selected application layout density.</summary>
    AppDensity Density { get; set; }

    /// <summary>The selected application color theme.</summary>
    AppTheme Theme { get; set; }
}
