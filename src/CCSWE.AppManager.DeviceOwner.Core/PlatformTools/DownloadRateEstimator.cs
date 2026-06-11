namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <summary>Smooths the download rate with an exponential moving average so the reported speed/ETA don't
/// jitter on every chunk. Time is supplied by the caller, keeping it pure and unit-testable.</summary>
internal sealed class DownloadRateEstimator
{
    private const double Alpha = 0.3;

    private double _bytesPerSecond;
    private bool _seeded;

    public double BytesPerSecond => _bytesPerSecond;

    public TimeSpan? Eta(long bytesRead, long? totalBytes)
    {
        if (totalBytes is not > 0 || _bytesPerSecond <= 0)
        {
            return null;
        }

        var remaining = totalBytes.Value - bytesRead;
        return remaining <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(remaining / _bytesPerSecond);
    }

    public double Update(long bytesDelta, double elapsedSeconds)
    {
        if (elapsedSeconds <= 0)
        {
            return _bytesPerSecond;
        }

        var instant = bytesDelta / elapsedSeconds;
        _bytesPerSecond = _seeded ? Alpha * instant + (1 - Alpha) * _bytesPerSecond : instant;
        _seeded = true;

        return _bytesPerSecond;
    }
}
