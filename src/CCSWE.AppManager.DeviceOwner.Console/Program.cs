using CCSWE.AppManager.DeviceOwner.Core;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

if (args.Contains("-h") || args.Contains("--help"))
{
    PrintUsage();
    return 0;
}

var requestedSerial = OptionValue(args, "--serial") ?? OptionValue(args, "-s");
var assumeYes = args.Contains("-y") || args.Contains("--yes");

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSimpleConsole(options => options.SingleLine = true).SetMinimumLevel(LogLevel.Warning));
services.AddDeviceOwnerCore();

using var provider = services.BuildServiceProvider();

var deviceService = provider.GetRequiredService<IDeviceService>();
var deviceOwnerService = provider.GetRequiredService<IDeviceOwnerService>();

if (!provider.GetRequiredService<IAdbLocator>().IsAvailable &&
    !await EnsureAdbAsync(provider.GetRequiredService<IPlatformToolsInstaller>(), assumeYes))
{
    return 1;
}

try
{
    Console.WriteLine("Scanning for devices...");
    var online = (await deviceService.ListAsync()).Where(device => device.IsOnline).ToList();

    if (online.Count == 0)
    {
        Console.Error.WriteLine("No online devices found. Connect a device (USB debugging enabled and authorized) and try again.");
        return 1;
    }

    var target = ResolveTarget(online, requestedSerial);
    if (target is null)
    {
        return 1;
    }

    if (!assumeYes && !Confirm(target, deviceOwnerService.Component))
    {
        Console.WriteLine("Cancelled.");
        return 0;
    }

    Console.WriteLine($"Setting device owner on {Describe(target)}...");
    var result = await deviceOwnerService.SetAsync(target.Serial);

    if (result.Success)
    {
        Console.WriteLine("Successfully set App Manager as the device owner.");
        return 0;
    }

    Console.Error.WriteLine("Failed to set device owner.");
    if (!string.IsNullOrWhiteSpace(result.Message))
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine(result.Message);
    }

    return 1;
}
catch (ProcessLaunchException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static bool Confirm(AdbDevice device, string component)
{
    Console.WriteLine();
    Console.WriteLine($"About to set the device owner to:");
    Console.WriteLine($"  {component}");
    Console.WriteLine($"on {Describe(device)}.");
    Console.Write("Continue? [y/N] ");

    var response = Console.ReadLine();
    return response is not null && (response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) || response.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase));
}

static bool ConfirmDownload()
{
    Console.Write("Download the Android platform tools now? [y/N] ");

    var response = Console.ReadLine();
    return response is not null && (response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) || response.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase));
}

static async Task<bool> EnsureAdbAsync(IPlatformToolsInstaller installer, bool assumeYes)
{
    Console.WriteLine("Android platform tools (adb) weren't found on this computer.");

    if (!installer.IsSupportedPlatform)
    {
        Console.Error.WriteLine("Automatic download isn't available on this platform. Install the Android SDK platform tools and ensure adb is on PATH.");
        return false;
    }

    if (!assumeYes && !ConfirmDownload())
    {
        Console.Error.WriteLine("adb is required. Install the Android SDK platform tools and try again.");
        return false;
    }

    Console.WriteLine("Downloading platform tools...");

    try
    {
        var adbPath = await installer.InstallAsync(new Progress<DownloadProgress>(RenderDownloadProgress));
        Console.WriteLine();
        Console.WriteLine($"Installed platform tools: {adbPath}");
        return true;
    }
    catch (Exception exception)
    {
        Console.WriteLine();
        Console.Error.WriteLine($"Failed to download platform tools: {exception.Message}");
        return false;
    }
}

static string Describe(AdbDevice device)
{
    var label = string.IsNullOrEmpty(device.Model) ? device.Serial : device.Model;
    return label == device.Serial ? device.Serial : $"{label} [{device.Serial}]";
}

static string? OptionValue(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static void RenderDownloadProgress(DownloadProgress progress)
{
    var read = DownloadFormat.Bytes(progress.BytesRead);
    var speed = progress.BytesPerSecond > 0 ? $"  {DownloadFormat.SpeedAndEta(progress.BytesPerSecond, progress.Eta)}" : string.Empty;
    var line = progress.TotalBytes is > 0
        ? $"  {read} / {DownloadFormat.Bytes(progress.TotalBytes.Value)}{speed}"
        : $"  {read}{speed}";

    Console.Write($"\r{line.PadRight(70)}");
}

static void PrintUsage()
{
    Console.WriteLine("CCSWE App Manager — Device Owner");
    Console.WriteLine();
    Console.WriteLine("Sets App Manager as the device owner on a connected device.");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  (no arguments)        Pick a device interactively, confirm, and set the device owner");
    Console.WriteLine("  --serial <serial>     Target this serial instead of prompting (alias: -s)");
    Console.WriteLine("  --yes                 Skip the confirmation prompt (alias: -y)");
    Console.WriteLine("  --help                Show this help (alias: -h)");
}

static AdbDevice? ResolveTarget(IReadOnlyList<AdbDevice> online, string? requestedSerial)
{
    if (!string.IsNullOrWhiteSpace(requestedSerial))
    {
        var match = online.FirstOrDefault(device => device.Serial == requestedSerial);
        if (match is null)
        {
            Console.Error.WriteLine($"No online device with serial '{requestedSerial}'.");
        }

        return match;
    }

    if (online.Count == 1)
    {
        return online[0];
    }

    Console.WriteLine();
    Console.WriteLine("Connected devices:");
    for (var i = 0; i < online.Count; i++)
    {
        Console.WriteLine($"  {i + 1}) {Describe(online[i])}");
    }

    Console.Write($"Select a device [1-{online.Count}]: ");
    var response = Console.ReadLine();

    if (int.TryParse(response, out var choice) && choice >= 1 && choice <= online.Count)
    {
        return online[choice - 1];
    }

    Console.Error.WriteLine("Invalid selection.");
    return null;
}
