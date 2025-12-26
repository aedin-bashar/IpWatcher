namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class IpHistoryRow
{
    public long Id { get; set; }
    public string IpText { get; set; } = null!;
    public string ChangedUtc { get; set; } = null!;
}
