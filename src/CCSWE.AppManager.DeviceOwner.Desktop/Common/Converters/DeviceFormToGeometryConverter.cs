using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CCSWE.AppManager.DeviceOwner.Core.Adb;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Common.Converters;

/// <summary>
/// Maps a <see cref="DeviceForm"/> to its Phosphor glyph from <c>Themes/Icons.axaml</c> (looked up by key from
/// the application resources). Mirrors the sibling Remote.Adb's tag-to-geometry converter.
/// </summary>
public sealed class DeviceFormToGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value is DeviceForm form
            ? form switch
            {
                DeviceForm.Tablet => "IconDeviceTablet",
                DeviceForm.Watch => "IconWatch",
                DeviceForm.Television => "IconTelevision",
                DeviceForm.Automotive => "IconCar",
                _ => "IconDeviceMobile",
            }
            : "IconDeviceMobile";

        if (Application.Current is { } application
            && application.TryGetResource(key, null, out var resource)
            && resource is Geometry geometry)
        {
            return geometry;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
