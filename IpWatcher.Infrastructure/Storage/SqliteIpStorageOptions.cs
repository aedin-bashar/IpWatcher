namespace IpWatcher.Infrastructure.Storage;

public sealed class SqliteIpStorageOptions
{
    // NOTE: For Windows Services, prefer an absolute path (e.g. under ProgramData),
    // because relative paths often resolve to C:\Windows\System32.
    public string ConnectionString { get; init; } = "Data Source=data/ipwatcher.sqlite";
}