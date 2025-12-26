using Microsoft.Extensions.Hosting;
using Serilog;

namespace IpWatcher.Worker.Logging;

public static class SerilogDatabaseLoggingExtensions
{
    public static HostApplicationBuilder AddDatabaseSerilogLogging(this HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<SqliteLogEventSink>();
        builder.Services.AddSerilog((services, lc) =>
        {
            lc.ReadFrom.Configuration(builder.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Sink(services.GetRequiredService<SqliteLogEventSink>());
        });
        return builder;
    }
}