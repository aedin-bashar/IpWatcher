using Microsoft.EntityFrameworkCore;

namespace IpWatcher.Infrastructure.Storage.EfCore;

public sealed class IpWatcherDbContext(DbContextOptions<IpWatcherDbContext> options) : DbContext(options)
{
    public DbSet<IpHistoryRow> IpHistory => Set<IpHistoryRow>();

    public DbSet<LogEventRow> LogEvents => Set<LogEventRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIpHistory(modelBuilder);
        ConfigureLogEvents(modelBuilder);
    }

    private static void ConfigureIpHistory(ModelBuilder modelBuilder)
    {
        var ipHistory = modelBuilder.Entity<IpHistoryRow>();

        ipHistory.ToTable("IpHistory");
        ipHistory.HasKey(x => x.Id);
        ipHistory.Property(x => x.Id).ValueGeneratedOnAdd();
        ipHistory.Property(x => x.IpText).IsRequired();
        ipHistory.Property(x => x.ChangedUtc).IsRequired();

        ipHistory.HasIndex(x => x.ChangedUtc)
            .HasDatabaseName("IX_IpHistory_ChangedUtc");
    }

    private static void ConfigureLogEvents(ModelBuilder modelBuilder)
    {
        var logs = modelBuilder.Entity<LogEventRow>();

        logs.ToTable("LogEvents");

        logs.HasKey(x => x.Id);
        logs.Property(x => x.Id).ValueGeneratedOnAdd();

        logs.Property(x => x.TimestampUtc).IsRequired();
        logs.Property(x => x.Level).IsRequired();
        logs.Property(x => x.RenderedMessage).IsRequired();
        logs.Property(x => x.LogEventJson).IsRequired();

        logs.HasIndex(x => x.TimestampUtc).HasDatabaseName("IX_LogEvents_TimestampUtc");
        logs.HasIndex(x => x.Level).HasDatabaseName("IX_LogEvents_Level");
    }
}
