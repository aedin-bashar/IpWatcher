using IpWatcher.Application.Abstractions;
using IpWatcher.Application.UseCases;
using IpWatcher.Infrastructure.Email;
using IpWatcher.Infrastructure.PublicIp;
using IpWatcher.Infrastructure.Storage.EfCore;
using IpWatcher.Worker.Scheduling;
using IpWatcher.Worker.Startup;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();

builder.Services.AddOptions<EmailOptions>()
	.Bind(builder.Configuration.GetSection("Email"));

builder.Services.AddDbContext<IpWatcherDbContext>((sp, options) =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("IpWatcher")
        ?? throw new InvalidOperationException("Missing connection string 'ConnectionStrings:IpWatcher'.");

    options.UseSqlite(
        connectionString,
        sqlite => sqlite.MigrationsAssembly(typeof(IpWatcherDbContext).Assembly.GetName().Name));
});

builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddHttpClient<IPublicIpProvider, IpifyPublicIpProvider>(client =>
{
	var baseUrl = builder.Configuration["Ipify:BaseUrl"];
	client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://api.ipify.org" : baseUrl);
});

builder.Services.AddScoped<IIpStorage, EfCoreIpStorage>();
builder.Services.AddSingleton<IEmailNotifier, MailKitEmailNotifier>();
builder.Services.AddTransient<CheckIpChangeUseCase>();
builder.Services.AddJobScheduling(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
