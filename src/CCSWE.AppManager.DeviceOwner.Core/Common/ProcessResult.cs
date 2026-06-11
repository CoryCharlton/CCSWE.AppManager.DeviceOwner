namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>
/// The captured outcome of running an external process to completion.
/// </summary>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>
    /// <see langword="true"/> when the process exited with code 0.
    /// </summary>
    public bool Success => ExitCode == 0;
}
