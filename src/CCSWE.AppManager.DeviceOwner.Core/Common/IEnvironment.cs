namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <summary>A thin seam over process <see cref="Environment"/> lookups, so consumers that resolve paths from
/// environment variables and known folders stay deterministically testable.</summary>
public interface IEnvironment
{
    /// <summary>Returns the value of the environment variable <paramref name="variable"/>, or <see langword="null"/>.</summary>
    string? GetEnvironmentVariable(string variable);

    /// <summary>Returns the path to the given <paramref name="folder"/>.</summary>
    string GetFolderPath(Environment.SpecialFolder folder);
}
