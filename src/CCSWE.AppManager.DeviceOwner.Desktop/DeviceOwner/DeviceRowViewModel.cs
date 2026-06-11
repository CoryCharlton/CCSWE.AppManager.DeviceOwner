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
        DisplayName = string.IsNullOrEmpty(device.Model) ? device.Serial : device.Model;
        Summary = string.Join(" · ", new[] { device.Product, device.Device }.Where(part => !string.IsNullOrEmpty(part)));
    }
}
