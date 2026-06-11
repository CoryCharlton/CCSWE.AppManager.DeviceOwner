using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <inheritdoc />
public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        // ArgumentList handles quoting/escaping per-argument, avoiding command-line injection bugs.
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static async Task ObserveAsync(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Swallow: we're already unwinding a cancellation; awaiting just marks the task observed.
        }
    }

    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        var startInfo = CreateStartInfo(fileName, arguments);

        _logger.LogDebug("Running {FileName} {Arguments}", fileName, string.Join(' ', arguments));

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Win32Exception exception)
        {
            throw new ProcessLaunchException(fileName, exception);
        }

        // Read both streams concurrently and only then wait for exit, otherwise a process that fills the
        // stderr pipe buffer while we drain stdout (or vice versa) would deadlock.
        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            return new ProcessResult(process.ExitCode, standardOutput, standardError);
        }
        catch (OperationCanceledException)
        {
            process.KillTree();
            await ObserveAsync(standardOutputTask);
            await ObserveAsync(standardErrorTask);
            throw;
        }
    }
}
