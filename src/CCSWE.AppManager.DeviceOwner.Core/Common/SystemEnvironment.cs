using System.Diagnostics.CodeAnalysis;

namespace CCSWE.AppManager.DeviceOwner.Core.Common;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public sealed class SystemEnvironment : IEnvironment
{
    /// <inheritdoc />
    public string? GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);

    /// <inheritdoc />
    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
}
