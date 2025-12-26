namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class LogEventRow
{
    public long Id { get; set; }

    public string TimestampUtc { get; set; } = null!;

    public string Level { get; set; } = null!;

    public string? MessageTemplate { get; set; }
    public string RenderedMessage { get; set; } = null!;

    public string? Exception { get; set; }

    public string? PropertiesJson { get; set; }

    public string LogEventJson { get; set; } = null!;
}