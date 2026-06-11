using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;

public partial class DownloadProgressDialogWindow : Window
{
    public DownloadProgressDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is DownloadProgressDialogViewModel viewModel)
        {
            _ = viewModel.RunAsync();
        }
    }
}
