using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;
using CCSWE.AppManager.DeviceOwner.Desktop.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;

/// <summary>
/// Drives the modal download dialog: starts the install, maps <see cref="DownloadProgress"/> onto bound
/// progress/speed/ETA properties (via a UI-thread-affine <see cref="Progress{T}"/>), and supports cancellation.
/// </summary>
public partial class DownloadProgressDialogViewModel : ViewModelBase, IDialogViewModel
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly IPlatformToolsInstaller _installer;

    [ObservableProperty]
    private bool _isIndeterminate = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CancelLabel))]
    private bool _isRunning = true;

    [ObservableProperty]
    private double _percentComplete;

    [ObservableProperty]
    private string _speedAndEtaLine = string.Empty;

    [ObservableProperty]
    private string _statusLine = "Connecting…";

    public DownloadProgressDialogViewModel(IPlatformToolsInstaller installer)
    {
        _installer = installer;
    }

    /// <summary>Raised when the dialog wants to close; the argument is whether the install succeeded.</summary>
    public event Action<bool>? CloseRequested;

    public string CancelLabel => IsRunning ? "Cancel" : "Close";

    public async Task RunAsync()
    {
        var progress = new Progress<DownloadProgress>(ApplyProgress);

        try
        {
            await _installer.InstallAsync(progress, _cancellation.Token);
            CloseRequested?.Invoke(true);
        }
        catch (OperationCanceledException exception) when (exception.CancellationToken == _cancellation.Token)
        {
            CloseRequested?.Invoke(false);
        }
        catch (Exception exception)
        {
            IsRunning = false;
            IsIndeterminate = false;
            PercentComplete = 0;
            StatusLine = DownloadError.Describe(exception);
            SpeedAndEtaLine = string.Empty;
        }
    }

    internal void ApplyProgress(DownloadProgress progress)
    {
        if (progress.TotalBytes is > 0)
        {
            IsIndeterminate = false;
            PercentComplete = progress.Fraction * 100;
            StatusLine = $"{DownloadFormat.Bytes(progress.BytesRead)} of {DownloadFormat.Bytes(progress.TotalBytes.Value)}";
        }
        else
        {
            IsIndeterminate = true;
            StatusLine = $"{DownloadFormat.Bytes(progress.BytesRead)} downloaded";
        }

        SpeedAndEtaLine = progress.BytesPerSecond > 0 ? DownloadFormat.SpeedAndEta(progress.BytesPerSecond, progress.Eta) : string.Empty;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (!IsRunning)
        {
            CloseRequested?.Invoke(false);
            return;
        }

        _cancellation.Cancel();
    }
}
