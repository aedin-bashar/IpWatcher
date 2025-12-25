using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace IpWatcher.Infrastructure.Storage;

public sealed class SqliteIpStorage(IOptions<SqliteIpStorageOptions> options) : IIpStorage
{
    private readonly SqliteIpStorageOptions _options = options.Value;

    public async Task<IpAddress?> LoadLastIpAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabasePathExistsAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT IpText
            FROM IpHistory
            ORDER BY Id DESC
            LIMIT 1;
            """;

        var ipText = (await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) as string;
        return string.IsNullOrWhiteSpace(ipText) ? null : IpAddress.Parse(ipText);
    }

    public async Task SaveLastIpAsync(IpAddress ipAddress, CancellationToken cancellationToken)
    {
        await EnsureDatabasePathExistsAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);

        await using var tx = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        // Safety: avoid duplicate rows if called twice with same IP.
        await using (var check = connection.CreateCommand())
        {
            check.Transaction = tx;
            check.CommandText = """
                SELECT IpText
                FROM IpHistory
                ORDER BY Id DESC
                LIMIT 1;
                """;

            var lastIpText = (await check.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false)) as string;
            if (!string.IsNullOrWhiteSpace(lastIpText) && string.Equals(lastIpText.Trim(), ipAddress.Value, StringComparison.Ordinal))
            {
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                return;
            }
        }

        await using (var insert = connection.CreateCommand())
        {
            insert.Transaction = tx;
            insert.CommandText = """
                INSERT INTO IpHistory (IpText, ChangedUtc)
                VALUES ($ip, $changedUtc);
                """;
            insert.Parameters.AddWithValue("$ip", ipAddress.Value);
            insert.Parameters.AddWithValue("$changedUtc", DateTimeOffset.UtcNow.ToString("O"));

            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS IpHistory
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IpText TEXT NOT NULL,
                ChangedUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_IpHistory_ChangedUtc ON IpHistory(ChangedUtc);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task EnsureDatabasePathExistsAsync(CancellationToken cancellationToken)
    {
        // If the connection string uses "Data Source=...", ensure its directory exists.
        var builder = new SqliteConnectionStringBuilder(_options.ConnectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource))
            return Task.CompletedTask;

        // Ignore in-memory dbs
        if (dataSource == ":memory:")
            return Task.CompletedTask;

        // If relative, make it absolute based on current process directory (service may be System32).
        var fullPath = Path.GetFullPath(dataSource);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        return Task.CompletedTask;
    }
}