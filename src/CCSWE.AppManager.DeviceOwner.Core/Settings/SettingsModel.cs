using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCSWE.AppManager.DeviceOwner.Core.Settings;

/// <summary>
/// The on-disk shape of persisted settings (serialized as camelCase JSON under the app-data folder). Kept
/// separate from <see cref="ISettingsService"/> so the file schema and the service surface can evolve
/// independently. Add a new setting by adding a property here (with a default) plus a member on the service.
/// </summary>
public sealed class SettingsModel
{
    /// <summary>Override for the <c>adb</c> executable path; <see langword="null"/> uses env-var/default/PATH resolution.</summary>
    public string? AdbPath { get; set; }

    /// <summary>The selected layout density. Serialized as its enum name.</summary>
    public AppDensity Density { get; set; } = AppDensity.Compact;

    /// <summary>
    /// Unknown keys carried in a newer build's settings file, preserved verbatim so an older build doesn't
    /// drop them when it re-saves.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; set; }

    /// <summary>The selected application color theme. Serialized as its enum name.</summary>
    public AppTheme Theme { get; set; } = AppTheme.Dark;
}
