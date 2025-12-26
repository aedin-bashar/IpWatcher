using Microsoft.EntityFrameworkCore;

namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class IpWatcherDbContext(DbContextOptions<IpWatcherDbContext> options) : DbContext(options)
{
    public DbSet<IpHistoryRow> IpHistory => Set<IpHistoryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<IpHistoryRow>();

        entity.ToTable("IpHistory");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedOnAdd();

        entity.Property(x => x.IpText).IsRequired();
        entity.Property(x => x.ChangedUtc).IsRequired();

        entity.HasIndex(x => x.ChangedUtc)
            .HasDatabaseName("IX_IpHistory_ChangedUtc");
    }
}
