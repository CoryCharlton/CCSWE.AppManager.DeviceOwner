using System.Collections.ObjectModel;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Desktop.Common;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;
using CCSWE.AppManager.DeviceOwner.Desktop.DeviceOwner;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Shell;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDeviceOwnerService _deviceOwnerService;
    private readonly IDeviceService _deviceService;
    private readonly INotificationService _notifications;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetDeviceOwnerCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetDeviceOwnerCommand))]
    private DeviceRowViewModel? _selectedDevice;

    [ObservableProperty]
    private string _statusText = "Scanning for devices…";

    public MainWindowViewModel(IDeviceService deviceService, IDeviceOwnerService deviceOwnerService, INotificationService notifications)
    {
        _deviceService = deviceService;
        _deviceOwnerService = deviceOwnerService;
        _notifications = notifications;

        Devices.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsListEmpty));
    }

    public bool CanRefresh => !IsBusy;

    public bool CanSetDeviceOwner => !IsBusy && SelectedDevice is not null;

    public ObservableCollection<DeviceRowViewModel> Devices { get; } = [];

    public bool IsListEmpty => Devices.Count == 0;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusText = "Scanning for devices…";

        try
        {
            var online = (await _deviceService.ListAsync()).Where(device => device.IsOnline).ToList();

            var selectedSerial = SelectedDevice?.Serial;
            Devices.Clear();
            foreach (var device in online)
            {
                Devices.Add(new DeviceRowViewModel(device));
            }

            SelectedDevice = Devices.FirstOrDefault(row => row.Serial == selectedSerial) ?? Devices.FirstOrDefault();

            StatusText = online.Count switch
            {
                0 => "No devices connected",
                1 => "1 device connected",
                _ => $"{online.Count} devices connected",
            };
        }
        catch (ProcessLaunchException exception)
        {
            StatusText = "adb not found";
            _notifications.Show("Couldn't list devices", exception.Message, NotificationSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSetDeviceOwner))]
    private async Task SetDeviceOwnerAsync()
    {
        var device = SelectedDevice;
        if (device is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _deviceOwnerService.SetAsync(device.Serial);

            if (result.Success)
            {
                StatusText = "Successfully set device owner";
                _notifications.Show("Device owner set", $"App Manager is now the device owner on {device.DisplayName}.", NotificationSeverity.Success);
            }
            else
            {
                StatusText = "Failed to set device owner";
                _notifications.Show("Couldn't set device owner", result.Message ?? "adb reported a failure.", NotificationSeverity.Error, TimeSpan.Zero);
            }
        }
        catch (ProcessLaunchException exception)
        {
            StatusText = "adb not found";
            _notifications.Show("Couldn't set device owner", exception.Message, NotificationSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
