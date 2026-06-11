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
}
