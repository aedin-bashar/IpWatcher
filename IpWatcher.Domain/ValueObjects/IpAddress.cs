namespace IpWatcher.Domain.ValueObjects;

public sealed record IpAddress
{
    public string Value { get; }

    private IpAddress(string value)
    {
        Value = value;
    }

    public static IpAddress Parse(string value)
    {
        if (!TryCreate(value, out var ipAddress))
        {
            throw new ArgumentException("Invalid IP address.", nameof(value));
        }

        return ipAddress;
    }

    public static bool TryCreate(string? value, out IpAddress ipAddress)
    {
        ipAddress = default!;

        var trimmed = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        if (!System.Net.IPAddress.TryParse(trimmed, out _))
        {
            return false;
        }

        ipAddress = new IpAddress(trimmed);
        return true;
    }

    public override string ToString() => Value;
}
