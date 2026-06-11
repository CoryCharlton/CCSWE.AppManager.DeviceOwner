namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <summary>A snapshot of an in-flight download: bytes transferred, the total (when known), and the
/// smoothed rate and estimated time remaining.</summary>
public readonly record struct DownloadProgress(long BytesRead, long? TotalBytes, double BytesPerSecond, TimeSpan? Eta)
{
    public double Fraction => TotalBytes is > 0 ? (double)BytesRead / TotalBytes.Value : 0;
}
