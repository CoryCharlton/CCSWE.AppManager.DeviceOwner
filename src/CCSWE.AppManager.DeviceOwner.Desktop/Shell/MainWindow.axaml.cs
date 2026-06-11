using Avalonia.Controls;
using Avalonia.Interactivity;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;

namespace CCSWE.AppManager.DeviceOwner.Desktop.Shell;

public partial class MainWindow : Window
{
    private readonly NotificationService? _notificationService;
    private bool _loaded;
    private bool _sinkAttached;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel, NotificationService notificationService) : this()
    {
        DataContext = viewModel;
        _notificationService = notificationService;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_notificationService is not null && !_sinkAttached)
        {
            _sinkAttached = true;
            _notificationService.SetSink(new WindowNotificationManagerSink(NotificationManager));
        }

        if (!_loaded && DataContext is MainWindowViewModel viewModel)
        {
            _loaded = true;
            viewModel.RefreshCommand.Execute(null);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _notificationService?.SetSink(null);
        _sinkAttached = false;

        base.OnUnloaded(e);
    }
}
