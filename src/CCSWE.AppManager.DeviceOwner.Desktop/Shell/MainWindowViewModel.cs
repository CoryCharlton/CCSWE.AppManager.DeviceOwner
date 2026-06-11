using System.Collections.ObjectModel;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Desktop.Common;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Threading;
using CCSWE.AppManager.DeviceOwner.Desktop.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Shell;

public partial class MainWindowViewModel : ViewModelBase
{
    // Background re-list cadence while the window is live, so a device plugged or unplugged shows up without a
    // manual refresh.
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(5);

    private readonly IAdbLocator _adbLocator;
    private readonly IConfirmDialog _confirmDialog;
    private readonly IDeviceOwnerPreflight _deviceOwnerPreflight;
    private readonly IDeviceOwnerService _deviceOwnerService;
    private readonly IDeviceService _deviceService;
    private readonly IPlatformToolsInstallDialog _installDialog;
    private readonly IMessageDialog _messageDialog;
    private readonly INotificationService _notifications;
    private readonly IDispatcherTimer _refreshTimer;
    private bool _isRefreshing;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetDeviceOwnerCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetDeviceOwnerCommand))]
    private DeviceRowViewModel? _selectedDevice;

    [ObservableProperty]
    private string _statusText = "Scanning for devices…";

    public MainWindowViewModel(IDeviceService deviceService, IDeviceOwnerService deviceOwnerService, IDeviceOwnerPreflight deviceOwnerPreflight, INotificationService notifications, IAdbLocator adbLocator, IConfirmDialog confirmDialog, IMessageDialog messageDialog, IPlatformToolsInstallDialog installDialog, ITimerFactory timerFactory)
    {
        _deviceService = deviceService;
        _deviceOwnerService = deviceOwnerService;
        _deviceOwnerPreflight = deviceOwnerPreflight;
        _notifications = notifications;
        _adbLocator = adbLocator;
        _confirmDialog = confirmDialog;
        _messageDialog = messageDialog;
        _installDialog = installDialog;

        _refreshTimer = timerFactory.Create(RefreshInterval);
        _refreshTimer.Tick += OnRefreshTick;

        Devices.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsListEmpty));
    }

    public bool CanRefresh => !IsBusy;

    public bool CanSetDeviceOwner => !IsBusy && SelectedDevice is not null;

    public ObservableCollection<DeviceRowViewModel> Devices { get; } = [];

    public bool IsListEmpty => Devices.Count == 0;

    private async Task<bool> EnsureAdbAsync()
    {
        if (_adbLocator.IsAvailable)
        {
            return true;
        }

        var confirmed = await _confirmDialog.ConfirmAsync(
            "adb not found",
            "Android platform tools (adb) weren't found on this computer. Download them now?",
            "Download");

        if (confirmed && await _installDialog.ShowAsync())
        {
            return true;
        }

        StatusText = "adb not found";
        return false;
    }

    // Lists online devices and reconciles them into the collection in place, so the selected device (matched by
    // serial) survives the refresh; selection only moves when nothing is selected or the selection disappeared.
    private async Task LoadAsync(bool notifyOnError)
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;

        try
        {
            var online = (await _deviceService.ListAsync()).Where(device => device.IsOnline).ToList();

            Devices.MergeBy(
                online,
                device => device.Serial,
                row => row.Serial,
                device => new DeviceRowViewModel(device),
                (row, device) => row.Update(device),
                row => row.DisplayName);

            if (SelectedDevice is null || Devices.All(row => row.Serial != SelectedDevice.Serial))
            {
                SelectedDevice = Devices.FirstOrDefault();
            }

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
            if (notifyOnError)
            {
                _notifications.Show("Couldn't list devices", exception.Message, NotificationSeverity.Error);
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // Background tick: re-list silently (no toast on a persistent failure) and never while a foreground operation
    // is running or adb is unavailable — so a tick never pops the install dialog.
    private void OnRefreshTick(object? sender, EventArgs e)
    {
        if (IsBusy || !_adbLocator.IsAvailable)
        {
            return;
        }

        _ = LoadAsync(notifyOnError: false);
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (!await EnsureAdbAsync())
        {
            return;
        }

        _refreshTimer.Start();

        IsBusy = true;
        StatusText = "Scanning for devices…";

        try
        {
            await LoadAsync(notifyOnError: true);
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
            var readiness = await _deviceOwnerPreflight.CheckAsync(device.Serial);

            if (readiness.AlreadyDeviceOwner)
            {
                StatusText = "Already the device owner";
                _notifications.Show("Already set", $"App Manager is already the device owner on {device.DisplayName}.", NotificationSeverity.Information);
                return;
            }

            if (!readiness.IsReady)
            {
                var reasons = string.Join("\n\n", readiness.Blockers.Select(blocker => blocker.Message));
                if (!await _confirmDialog.ConfirmAsync("Device may not be ready", reasons, "Try anyway"))
                {
                    StatusText = "Device not ready";
                    return;
                }
            }

            var result = await _deviceOwnerService.SetAsync(device.Serial);

            if (result.Success)
            {
                StatusText = "Successfully set device owner";
                _notifications.Show("Device owner set", $"App Manager is now the device owner on {device.DisplayName}.", NotificationSeverity.Success);
            }
            else
            {
                StatusText = "Failed to set device owner";
                await _messageDialog.ShowAsync("Couldn't set device owner", result.Message ?? "adb reported a failure.");
            }
        }
        catch (ProcessLaunchException exception)
        {
            StatusText = "adb not found";
            await _messageDialog.ShowAsync("Couldn't set device owner", exception.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
