namespace CCSWE.AppManager.DeviceOwner.Core.Adb;

/// <summary>
/// Pure parsing of <c>adb</c> command output. Kept free of I/O so it is trivially unit-testable. Parses over
/// spans, allocating only the field strings it keeps.
/// </summary>
public static class AdbOutputParser
{
    /// <summary>
    /// Parses the output of <c>adb devices -l</c> into devices, preserving every connection state
    /// (<c>device</c>, <c>offline</c>, <c>unauthorized</c>, …). The trailing <c>key:value</c> columns
    /// (<c>model</c>, <c>product</c>, <c>device</c>, <c>transport_id</c>) are captured when present.
    /// </summary>
    public static IReadOnlyList<AdbDevice> ParseDeviceList(string output)
    {
        var devices = new List<AdbDevice>();

        foreach (var rawLine in output.AsSpan().EnumerateLines())
        {
            var line = rawLine.Trim();

            if (line.IsEmpty || line.StartsWith("List of devices", StringComparison.Ordinal))
            {
                continue;
            }

            var rest = line;
            var serial = NextToken(ref rest);
            var state = NextToken(ref rest);

            if (serial.IsEmpty || state.IsEmpty)
            {
                continue;
            }

            string? model = null;
            string? product = null;
            string? device = null;
            string? transportId = null;

            for (var token = NextToken(ref rest); !token.IsEmpty; token = NextToken(ref rest))
            {
                var colon = token.IndexOf(':');
                if (colon <= 0)
                {
                    continue;
                }

                var key = token[..colon];
                var value = token[(colon + 1)..];

                if (key.SequenceEqual("model"))
                {
                    model = value.ToString();
                }
                else if (key.SequenceEqual("product"))
                {
                    product = value.ToString();
                }
                else if (key.SequenceEqual("device"))
                {
                    device = value.ToString();
                }
                else if (key.SequenceEqual("transport_id"))
                {
                    transportId = value.ToString();
                }
            }

            devices.Add(new AdbDevice(serial.ToString(), state.ToString(), model, product, device, transportId));
        }

        return devices;
    }

    /// <summary>
    /// Parses <c>dpm list-owners</c> into the owners it reports. Empty/"no owners" output yields an empty list.
    /// Each owner line is <c>User &lt;id&gt;: admin=&lt;component&gt;,&lt;flag&gt;,…</c>; the flags carry the
    /// <c>DeviceOwner</c>/<c>ProfileOwner</c>/<c>ManagedProfileOwner(…)</c> roles.
    /// </summary>
    public static IReadOnlyList<AdbOwner> ParseOwners(string output)
    {
        var owners = new List<AdbOwner>();

        foreach (var rawLine in output.AsSpan().EnumerateLines())
        {
            var line = rawLine.Trim();

            var adminIndex = line.IndexOf("admin=", StringComparison.Ordinal);
            if (adminIndex < 0)
            {
                continue;
            }

            var userId = ParseUserId(line);

            var afterAdmin = line[(adminIndex + "admin=".Length)..];
            var comma = afterAdmin.IndexOf(',');
            var component = (comma < 0 ? afterAdmin : afterAdmin[..comma]).Trim();
            var flags = comma < 0 ? [] : afterAdmin[(comma + 1)..];

            var (isDeviceOwner, isProfileOwner) = ReadOwnerFlags(flags);

            var slash = component.IndexOf('/');
            var package = (slash <= 0 ? component : component[..slash]).Trim();

            owners.Add(new AdbOwner(userId, component.ToString(), package.ToString(), isDeviceOwner, isProfileOwner));
        }

        return owners;
    }

    /// <summary>Counts the users reported by <c>pm list users</c> (one <c>UserInfo{…}</c> per user).</summary>
    public static int ParseUserCount(string output)
    {
        var count = 0;

        foreach (var rawLine in output.AsSpan().EnumerateLines())
        {
            if (rawLine.Contains("UserInfo{", StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Sums the <c>Accounts: &lt;N&gt;</c> lines from <c>dumpsys account</c>. Returns <see langword="null"/> when
    /// no such line is present (older Android), where the count is unknown rather than zero.
    /// </summary>
    public static int? ParseAccountCount(string output)
    {
        int? total = null;

        foreach (var rawLine in output.AsSpan().EnumerateLines())
        {
            var line = rawLine.Trim();

            if (!line.StartsWith("Accounts:", StringComparison.Ordinal))
            {
                continue;
            }

            if (int.TryParse(line["Accounts:".Length..].Trim(), out var count))
            {
                total = (total ?? 0) + count;
            }
        }

        return total;
    }

    // Returns the next space/tab-delimited token, advancing `remaining` past it; an empty span when none remain.
    private static ReadOnlySpan<char> NextToken(ref ReadOnlySpan<char> remaining)
    {
        var i = 0;
        while (i < remaining.Length && remaining[i] is ' ' or '\t')
        {
            i++;
        }

        var start = i;
        while (i < remaining.Length && remaining[i] is not (' ' or '\t'))
        {
            i++;
        }

        var token = remaining[start..i];
        remaining = remaining[i..];
        return token;
    }

    // Reads the comma-separated owner flags, distinguishing the device-owner role from the profile-owner roles
    // (DeviceOwner is its own token; ProfileOwner and ManagedProfileOwner(…) both mean a profile owner).
    private static (bool IsDeviceOwner, bool IsProfileOwner) ReadOwnerFlags(ReadOnlySpan<char> flags)
    {
        var isDeviceOwner = false;
        var isProfileOwner = false;

        while (!flags.IsEmpty)
        {
            var comma = flags.IndexOf(',');
            var token = (comma < 0 ? flags : flags[..comma]).Trim();

            if (token.SequenceEqual("DeviceOwner"))
            {
                isDeviceOwner = true;
            }
            else if (token.SequenceEqual("ProfileOwner") || token.StartsWith("ManagedProfileOwner", StringComparison.Ordinal))
            {
                isProfileOwner = true;
            }

            flags = comma < 0 ? [] : flags[(comma + 1)..];
        }

        return (isDeviceOwner, isProfileOwner);
    }

    // Extracts the user id from a leading "User <id>:" prefix, or null when the line isn't user-scoped.
    private static int? ParseUserId(ReadOnlySpan<char> line)
    {
        if (!line.StartsWith("User", StringComparison.Ordinal))
        {
            return null;
        }

        var rest = line[4..];
        var i = 0;
        while (i < rest.Length && rest[i] == ' ')
        {
            i++;
        }

        var start = i;
        while (i < rest.Length && char.IsDigit(rest[i]))
        {
            i++;
        }

        return i > start ? int.Parse(rest[start..i]) : null;
    }
}
