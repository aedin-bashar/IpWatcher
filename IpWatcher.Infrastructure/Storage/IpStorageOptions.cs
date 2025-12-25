namespace IpWatcher.Infrastructure.Storage;

public sealed class IpStorageOptions
{
    public string FilePath { get; init; } = "ipwatcher.lastip.txt";
}
