using Microsoft.Data.Sqlite;

namespace IpWatcher.Worker.Startup;

internal static class SqliteConnectionStringHelper
{
    public static string ResolveToWritablePath(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
            return connectionString;

        if (Path.IsPathRooted(dataSource))
            return connectionString;

        var fullPath = Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        builder.DataSource = fullPath;
        return builder.ToString();
    }
}
