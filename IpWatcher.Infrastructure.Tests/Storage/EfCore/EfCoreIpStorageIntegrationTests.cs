using IpWatcher.Domain.ValueObjects;
using IpWatcher.Infrastructure.Storage.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace IpWatcher.Infrastructure.Tests.Storage.EfCore;

public sealed class EfCoreIpStorageIntegrationTests
{
    private sealed class DbFixture : IAsyncDisposable
    {
        public DbFixture(IpWatcherDbContext db, string rootPath)
        {
            Db = db;
            RootPath = rootPath;
        }

        public IpWatcherDbContext Db { get; }
        public string RootPath { get; }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync().ConfigureAwait(false);

            try
            {
                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
                // Best-effort cleanup only.
            }
        }
    }

    private static async Task<DbFixture> CreateDbAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "IpWatcher.Infrastructure.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var dbPath = Path.Combine(root, "ipwatcher.test.db");

        var options = new DbContextOptionsBuilder<IpWatcherDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        var db = new IpWatcherDbContext(options);
        await db.Database.MigrateAsync().ConfigureAwait(false);

        return new DbFixture(db, root);
    }

    [Fact]
    public async Task LoadLastIpAsync_WhenDatabaseEmpty_ReturnsNull()
    {
        // Arrange
        await using var setup = await CreateDbAsync();
        var db = setup.Db;

        var storage = new EfCoreIpStorage(db, NullLogger<EfCoreIpStorage>.Instance);

        // Act
        var last = await storage.LoadLastIpAsync(CancellationToken.None);

        // Asert
        Assert.Null(last);
    }

    [Fact]
    public async Task SaveLastIpAsync_WhenNoRows_InsertsRow_AndLoadReturnsSame()
    {
        // Arrange
        await using var setup = await CreateDbAsync();
        var db = setup.Db;

        var storage = new EfCoreIpStorage(db, NullLogger<EfCoreIpStorage>.Instance);
        var ip = IpAddress.Parse("1.1.1.1");

        // Act
        await storage.SaveLastIpAsync(ip, CancellationToken.None);
        var loaded = await storage.LoadLastIpAsync(CancellationToken.None);
        var count = await db.IpHistory.CountAsync();

        // Asert
        Assert.Equal(ip, loaded);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task SaveLastIpAsync_WhenSameAsLatest_DoesNotInsertDuplicate()
    {
        // Arrange
        await using var setup = await CreateDbAsync();
        var db = setup.Db;

        var storage = new EfCoreIpStorage(db, NullLogger<EfCoreIpStorage>.Instance);
        var ip = IpAddress.Parse("1.1.1.1");

        await storage.SaveLastIpAsync(ip, CancellationToken.None);
        var countAfterFirst = await db.IpHistory.CountAsync();

        // Act
        await storage.SaveLastIpAsync(ip, CancellationToken.None);
        var countAfterSecond = await db.IpHistory.CountAsync();
        var loaded = await storage.LoadLastIpAsync(CancellationToken.None);

        // Asert
        Assert.Equal(1, countAfterFirst);
        Assert.Equal(1, countAfterSecond);
        Assert.Equal(ip, loaded);
    }

    [Fact]
    public async Task SaveLastIpAsync_WhenDifferentFromLatest_InsertsNewRow_AndLoadReturnsNew()
    {
        // Arrange
        await using var setup = await CreateDbAsync();
        var db = setup.Db;

        var storage = new EfCoreIpStorage(db, NullLogger<EfCoreIpStorage>.Instance);
        var first = IpAddress.Parse("1.1.1.1");
        var second = IpAddress.Parse("2.2.2.2");

        await storage.SaveLastIpAsync(first, CancellationToken.None);

        // Act
        await storage.SaveLastIpAsync(second, CancellationToken.None);
        var loaded = await storage.LoadLastIpAsync(CancellationToken.None);
        var count = await db.IpHistory.CountAsync();

        // Asert
        Assert.Equal(2, count);
        Assert.Equal(second, loaded);
    }
}