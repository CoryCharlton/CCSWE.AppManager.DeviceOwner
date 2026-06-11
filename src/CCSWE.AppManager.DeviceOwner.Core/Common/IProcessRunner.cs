namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// Runs an external command-line tool (<c>adb</c>) to completion and captures its output.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs <paramref name="fileName"/> with <paramref name="arguments"/> to completion and returns its
    /// captured exit code and output.
    /// </summary>
    Task<ProcessResult> RunAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
}
