using System.ComponentModel;
using System.Diagnostics;

namespace CCSWE.AppManager.DeviceOwner.Core.Common;

internal static class ProcessExtensions
{
    /// <summary>
    /// Kills the process and its descendants if it is still running. Safe on an already-exited or unkillable
    /// process — the platform exceptions that signal "nothing to kill" are swallowed.
    /// </summary>
    public static void KillTree(this Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or NotSupportedException)
        {
        }
    }
}
