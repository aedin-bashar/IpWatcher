using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class EfCoreIpStorage(IpWatcherDbContext db, ILogger<EfCoreIpStorage> logger) : IIpStorage
{
    public async Task<IpAddress?> LoadLastIpAsync(CancellationToken cancellationToken)
    {
        var ipText = await db.IpHistory
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => x.IpText)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(ipText))
        {
            logger.LogInformation("No IP history found in database.");
            return null;
        }

        return IpAddress.Parse(ipText);
    }

    public async Task SaveLastIpAsync(IpAddress ipAddress, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var lastIpText = await db.IpHistory
            .OrderByDescending(x => x.Id)
            .Select(x => x.IpText)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(lastIpText) &&
            string.Equals(lastIpText.Trim(), ipAddress.Value, StringComparison.Ordinal))
        {
            logger.LogInformation("IP {CurrentIp} already stored as latest. Skipping insert.", ipAddress.Value);
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

        logger.LogInformation("Stored new IP history row: {CurrentIp}.", ipAddress.Value);
    }
}
