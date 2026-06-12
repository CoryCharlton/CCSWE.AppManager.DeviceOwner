using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CCSWE.AppManager.DeviceOwner.Desktop.DeviceOwner;

/// <summary>
/// A single selectable device in the picker: a bindable projection of <see cref="AdbDevice"/>, keyed by
/// <see cref="Serial"/> so the selection survives a refresh.
/// </summary>
public partial class DeviceRowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private DeviceForm _form;

    [ObservableProperty]
    private bool _isEmulator;

    [ObservableProperty]
    private string? _summary;

    public DeviceRowViewModel(AdbDevice device)
    {
        Serial = device.Serial;
        Update(device);
    }

    /// <summary>The adb serial — the immutable identity used to match rows across refreshes.</summary>
    public string Serial { get; }

    /// <summary>Applies the latest snapshot to this row, preserving its identity.</summary>
    public void Update(AdbDevice device)
    {
        DisplayName = FriendlyName(device);
        Form = device.Form;
        IsEmulator = device.IsEmulator;
        Summary = string.Join(" · ", new[] { device.Product, device.Device }.Where(part => !string.IsNullOrEmpty(part)));
    }

    // Prefer the resolved name; otherwise the adb model (de-sanitized — adb replaces spaces with underscores);
    // otherwise the serial.
    private static string FriendlyName(AdbDevice device)
    {
        if (!string.IsNullOrEmpty(device.Name))
        {
            return device.Name;
        }

        return string.IsNullOrEmpty(device.Model) ? device.Serial : device.Model.Replace('_', ' ');
    }
}
