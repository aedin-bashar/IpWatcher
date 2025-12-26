using IpWatcher.Infrastructure.Storage.EfCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IpWatcher.Worker.Startup;

public sealed class DatabaseMigrationHostedService(
    IServiceProvider services,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connectionString =
            configuration.GetConnectionString("IpWatcher")
            ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:IpWatcher'.");

        connectionString = SqliteConnectionStringHelper.ResolveToWritablePath(
            connectionString,
            environment.ContentRootPath);

        EnsureDatabaseDirectoryExists(connectionString);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IpWatcherDbContext>();

        logger.LogInformation("Applying EF Core migrations (SQLite)...");
        await db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("EF Core migrations applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void EnsureDatabaseDirectoryExists(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
            return;

        var fullPath = Path.GetFullPath(dataSource);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}