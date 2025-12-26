using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class EfCoreIpStorage(IpWatcherDbContext db) : IIpStorage
{
    public async Task<IpAddress?> LoadLastIpAsync(CancellationToken cancellationToken)
    {
        var ipText = await db.IpHistory
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.IpText)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(ipText) ? null : IpAddress.Parse(ipText);
    }

    public async Task SaveLastIpAsync(IpAddress ipAddress, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var lastIpText = await db.IpHistory
            .OrderByDescending(x => x.Id)
            .Select(x => x.IpText)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(lastIpText) && string.Equals(lastIpText.Trim(), ipAddress.Value, StringComparison.Ordinal))
        {
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        db.IpHistory.Add(new IpHistoryRow
        {
            IpText = ipAddress.Value,
            ChangedUtc = DateTimeOffset.UtcNow.ToString("O")
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
