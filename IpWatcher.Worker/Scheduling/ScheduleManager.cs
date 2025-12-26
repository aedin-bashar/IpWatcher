using IpWatcher.Worker.Jobs;
using Quartz;

namespace IpWatcher.Worker.Scheduling;

public static class ScheduleManager
{
    public static IServiceCollection AddJobScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        var intervalMinutes = configuration.GetValue<int?>("Jobs:CheckIpChange:IntervalMinutes") ?? 5;
        if (intervalMinutes <= 0)
            throw new InvalidOperationException("Jobs:CheckIpChange:IntervalMinutes must be > 0.");

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(CheckIpChangeJob));

            q.AddJob<CheckIpChangeJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(CheckIpChangeJob)}-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromMinutes(intervalMinutes))
                    .RepeatForever()));
        });

        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

        return services;
    }
}