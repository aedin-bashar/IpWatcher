using IpWatcher.Worker.Startup;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IpWatcher.Worker.Tests.Startup;

public sealed class SqliteConnectionStringHelperTests
{
    [Fact]
    public void ResolveToWritablePath_WhenMemoryDb_ReturnsOriginal()
    {
        // Arrange
        var original = "Data Source=:memory:";

        // Act
        var resolved = SqliteConnectionStringHelper.ResolveToWritablePath(original, contentRootPath: @"C:\any");

        // Asert
        // Compare by DataSource to avoid connection-string normalization differences.
        Assert.Equal(
            new SqliteConnectionStringBuilder(original).DataSource,
            new SqliteConnectionStringBuilder(resolved).DataSource);
    }

    [Fact]
    public void ResolveToWritablePath_WhenRootedPath_ReturnsOriginalDataSource()
    {
        // Arrange
        var original = @"Data Source=C:\Temp\ipwatcher\ipwatcher.db";

        // Act
        var resolved = SqliteConnectionStringHelper.ResolveToWritablePath(original, contentRootPath: @"C:\root");

        // Asert
        Assert.Equal(
            new SqliteConnectionStringBuilder(original).DataSource,
            new SqliteConnectionStringBuilder(resolved).DataSource);
    }

    [Fact]
    public void ResolveToWritablePath_WhenRelativePath_MakesAbsoluteUnderContentRoot_AndCreatesDirectory()
    {
        // Arrange
        var contentRoot = Path.Combine(Path.GetTempPath(), "IpWatcher.Worker.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contentRoot);

        var relative = Path.Combine("data", "ipwatcher.db");
        var original = $"Data Source={relative}";

        var expectedFullPath = Path.GetFullPath(Path.Combine(contentRoot, relative));
        var expectedDir = Path.GetDirectoryName(expectedFullPath)!;

        // Ensure the directory doesn't exist before the call (test the side effect).
        if (Directory.Exists(expectedDir))
            Directory.Delete(expectedDir, recursive: true);

        // Act
        var resolved = SqliteConnectionStringHelper.ResolveToWritablePath(original, contentRoot);

        // Asert
        var resolvedBuilder = new SqliteConnectionStringBuilder(resolved);
        Assert.Equal(expectedFullPath, resolvedBuilder.DataSource);
        Assert.True(Directory.Exists(expectedDir));
    }
}