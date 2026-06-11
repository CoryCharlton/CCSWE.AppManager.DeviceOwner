using System.Diagnostics;
using System.IO.Compression;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

/// <inheritdoc />
public sealed class PlatformToolsInstaller : IPlatformToolsInstaller
{
    private const string PlatformToolsFolder = "platform-tools";

    private static readonly TimeSpan ProgressInterval = TimeSpan.FromMilliseconds(100);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _installRoot;
    private readonly ILogger<PlatformToolsInstaller> _logger;
    private readonly ISettingsService _settings;

    public PlatformToolsInstaller(ISettingsService settings, IHttpClientFactory httpClientFactory, ILogger<PlatformToolsInstaller> logger, string? installRoot = null)
    {
        _settings = settings;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _installRoot = installRoot ?? InstallRoot();
    }

    public bool IsSupportedPlatform => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();

    public async Task<string> InstallAsync(IProgress<DownloadProgress>? progress, CancellationToken cancellationToken = default)
    {
        if (!IsSupportedPlatform)
        {
            throw new PlatformNotSupportedException("Android platform tools are only available for Windows, macOS, and Linux.");
        }

        var adbPath = AdbPath();
        if (File.Exists(adbPath))
        {
            _logger.LogDebug("Platform tools already installed at {Path}; reusing.", adbPath);
            _settings.AdbPath = adbPath;
            return adbPath;
        }

        var url = DownloadUrlFor(OperatingSystem.IsWindows(), OperatingSystem.IsMacOS(), OperatingSystem.IsLinux());
        var zipPath = Path.Combine(Path.GetTempPath(), $"platform-tools-{Guid.NewGuid():N}.zip");

        try
        {
            await DownloadAsync(url, zipPath, progress, cancellationToken);

            await using var zip = File.OpenRead(zipPath);
            return await ExtractAndRegisterAsync(zip, cancellationToken);
        }
        finally
        {
            TryDelete(zipPath);
        }
    }

    internal static string DownloadUrlFor(bool isWindows, bool isMacOs, bool isLinux)
    {
        const string baseUrl = "https://dl.google.com/android/repository/platform-tools-latest-";

        if (isWindows)
        {
            return $"{baseUrl}windows.zip";
        }

        if (isMacOs)
        {
            return $"{baseUrl}darwin.zip";
        }

        if (isLinux)
        {
            return $"{baseUrl}linux.zip";
        }

        throw new PlatformNotSupportedException("No Android platform tools archive is published for this platform.");
    }

    internal static string InstallRoot()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "CCSWE.AppManager.DeviceOwner");
    }

    internal async Task<string> ExtractAndRegisterAsync(Stream zip, CancellationToken cancellationToken)
    {
        var stagingRoot = Path.Combine(_installRoot, ".staging");
        var stagedTools = Path.Combine(stagingRoot, PlatformToolsFolder);
        var installedTools = Path.Combine(_installRoot, PlatformToolsFolder);

        Directory.CreateDirectory(_installRoot);
        DeleteDirectory(stagingRoot);

        ZipFile.ExtractToDirectory(zip, stagingRoot, overwriteFiles: true);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(stagedTools))
        {
            throw new InvalidOperationException($"The downloaded archive did not contain a '{PlatformToolsFolder}' folder.");
        }

        DeleteDirectory(installedTools);
        Directory.Move(stagedTools, installedTools);
        DeleteDirectory(stagingRoot);

        MarkExecutable(installedTools);

        var adbPath = AdbPath();
        if (!File.Exists(adbPath))
        {
            throw new InvalidOperationException($"adb was not found at '{adbPath}' after extraction.");
        }

        _settings.AdbPath = adbPath;
        _logger.LogInformation("Installed platform tools to {Path}.", adbPath);

        return adbPath;
    }

    private static void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static void MarkExecutable(string platformToolsDirectory)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        const UnixFileMode mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                  UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                                  UnixFileMode.OtherRead | UnixFileMode.OtherExecute;

        foreach (var file in Directory.EnumerateFiles(platformToolsDirectory))
        {
            if (string.IsNullOrEmpty(Path.GetExtension(file)))
            {
                File.SetUnixFileMode(file, mode);
            }
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
    }

    private string AdbPath()
    {
        var adb = OperatingSystem.IsWindows() ? "adb.exe" : "adb";
        return Path.Combine(_installRoot, PlatformToolsFolder, adb);
    }

    private async Task DownloadAsync(string url, string zipPath, IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(PlatformToolsInstaller));

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var estimator = new DownloadRateEstimator();
        var stopwatch = Stopwatch.StartNew();
        var lastReport = TimeSpan.Zero;
        var lastReportBytes = 0L;
        var buffer = new byte[81920];
        var totalRead = 0L;

        progress?.Report(new DownloadProgress(0, totalBytes, 0, null));

        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            var elapsed = stopwatch.Elapsed;
            if (elapsed - lastReport < ProgressInterval)
            {
                continue;
            }

            var bytesPerSecond = estimator.Update(totalRead - lastReportBytes, (elapsed - lastReport).TotalSeconds);
            progress?.Report(new DownloadProgress(totalRead, totalBytes, bytesPerSecond, estimator.Eta(totalRead, totalBytes)));

            lastReport = elapsed;
            lastReportBytes = totalRead;
        }

        progress?.Report(new DownloadProgress(totalRead, totalBytes ?? totalRead, estimator.BytesPerSecond, TimeSpan.Zero));
    }
}
