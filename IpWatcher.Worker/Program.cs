using IpWatcher.Application.Abstractions;
using IpWatcher.Application.UseCases;
using IpWatcher.Infrastructure.Email;
using IpWatcher.Infrastructure.Storage.EfCore;
using IpWatcher.Worker.Logging;
using IpWatcher.Worker.Scheduling;
using IpWatcher.Worker.Startup;

var builder = Host.CreateApplicationBuilder(args);

// Host/logging first
builder.AddDatabaseSerilogLogging();
builder.Services.AddWindowsService();

// Config / infrastructure wiring
builder.AddEmailOptions();
builder.AddIpWatcherSqliteDb();
builder.AddPublicIpProvider();

// Hosted services + app services
builder.Services.AddHostedService<DatabaseMigrationHostedService>();

builder.Services.AddScoped<IIpStorage, EfCoreIpStorage>();
builder.Services.AddSingleton<IEmailNotifier, MailKitEmailNotifier>();
builder.Services.AddTransient<CheckIpChangeUseCase>();

// Scheduling
builder.Services.AddJobScheduling(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
