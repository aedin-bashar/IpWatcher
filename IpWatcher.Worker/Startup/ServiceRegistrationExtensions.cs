using IpWatcher.Application.Abstractions;
using IpWatcher.Infrastructure.Email;
using IpWatcher.Infrastructure.PublicIp;
using IpWatcher.Infrastructure.Storage.EfCore;
using Microsoft.EntityFrameworkCore;

namespace IpWatcher.Worker.Startup;

public static class ServiceRegistrationExtensions
{
    public static HostApplicationBuilder AddEmailOptions(this HostApplicationBuilder builder)
    {
        builder.Services.AddOptions<EmailOptions>()
            .Bind(builder.Configuration.GetSection("Email"));

        return builder;
    }

    public static HostApplicationBuilder AddIpWatcherSqliteDb(this HostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<IpWatcherDbContext>((sp, options) =>
        {
            var connectionString =
                builder.Configuration.GetConnectionString("IpWatcher")
                ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:IpWatcher'.");

            connectionString = SqliteConnectionStringHelper.ResolveToWritablePath(
                connectionString,
                builder.Environment.ContentRootPath);

            options.UseSqlite(
                connectionString,
                sqlite => sqlite.MigrationsAssembly(typeof(IpWatcherDbContext).Assembly.GetName().Name));
        });

        return builder;
    }

    public static HostApplicationBuilder AddPublicIpProvider(this HostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<IPublicIpProvider, IpifyPublicIpProvider>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var baseUrl = configuration["Ipify:BaseUrl"];
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://api.ipify.org" : baseUrl);
        });

        return builder;
    }
}