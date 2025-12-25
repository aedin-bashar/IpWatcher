using IpWatcher.Application.Abstractions;
using IpWatcher.Application.UseCases;
using IpWatcher.Infrastructure.Email;
using IpWatcher.Infrastructure.PublicIp;
using IpWatcher.Infrastructure.Storage;
using IpWatcher.Worker.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService();

builder.Services.AddOptions<EmailOptions>()
	.Bind(builder.Configuration.GetSection("Email"));

// NEW: SQLite options
builder.Services.AddOptions<SqliteIpStorageOptions>()
	.Bind(builder.Configuration.GetSection("IpStorageSqlite"));

builder.Services.AddHttpClient<IPublicIpProvider, IpifyPublicIpProvider>(client =>
{
	var baseUrl = builder.Configuration["Ipify:BaseUrl"];
	client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? "https://api.ipify.org" : baseUrl);
});

// CHANGE: use SQLite storage instead of file storage
builder.Services.AddSingleton<IIpStorage, SqliteIpStorage>();

builder.Services.AddSingleton<IEmailNotifier, MailKitEmailNotifier>();
builder.Services.AddTransient<CheckIpChangeUseCase>();

builder.Services.AddQuartz(q =>
{
	var jobKey = new JobKey(nameof(CheckIpChangeJob));
	q.AddJob<CheckIpChangeJob>(opts => opts.WithIdentity(jobKey));
	q.AddTrigger(opts => opts
		.ForJob(jobKey)
		.WithIdentity($"{nameof(CheckIpChangeJob)}-trigger")
		.StartNow()
		.WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(5)).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
	options.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();
