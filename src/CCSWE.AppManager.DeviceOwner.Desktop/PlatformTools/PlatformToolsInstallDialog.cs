using CCSWE.AppManager.DeviceOwner.Desktop.Common;

namespace CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;

/// <inheritdoc />
public sealed class PlatformToolsInstallDialog : IPlatformToolsInstallDialog
{
    private readonly Func<DownloadProgressDialogViewModel> _viewModelFactory;

    public PlatformToolsInstallDialog(Func<DownloadProgressDialogViewModel> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    /// <inheritdoc />
    public Task<bool> ShowAsync() => DialogHost.ShowAsync<DownloadProgressDialogWindow>(_viewModelFactory());
}
