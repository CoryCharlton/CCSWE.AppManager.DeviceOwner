namespace CCSWE.AppManager.DeviceOwner.Core.Settings;

/// <summary>
/// <see cref="ISettingsService"/> backed by an <see cref="ISettingsStore"/>: the persisted model is loaded
/// once on construction and re-saved whenever a setting changes.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly SettingsModel _model;
    private readonly ISettingsStore _store;

    public SettingsService(ISettingsStore store)
    {
        _store = store;
        _model = _store.Load();
    }

    public string? AdbPath
    {
        get => _model.AdbPath;
        set => SetOverride(_model.AdbPath, value, normalized => _model.AdbPath = normalized);
    }

    public AppDensity Density
    {
        get => _model.Density;
        set
        {
            if (_model.Density == value)
            {
                return;
            }

            _model.Density = value;
            _store.Save(_model);
        }
    }

    public AppTheme Theme
    {
        get => _model.Theme;
        set
        {
            if (_model.Theme == value)
            {
                return;
            }

            _model.Theme = value;
            _store.Save(_model);
        }
    }

    // A blank or whitespace path means "no override" — store it as null so it round-trips as absent.
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void SetOverride(string? current, string? value, Action<string?> assign)
    {
        var normalized = Normalize(value);
        if (current == normalized)
        {
            return;
        }

        assign(normalized);
        _store.Save(_model);
    }
}
