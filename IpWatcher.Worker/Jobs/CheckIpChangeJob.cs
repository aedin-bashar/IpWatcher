using IpWatcher.Application.UseCases;
using Quartz;

namespace IpWatcher.Worker.Jobs;

public sealed class CheckIpChangeJob(CheckIpChangeUseCase useCase) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // The job must only call the use case.
        return useCase.ExecuteAsync(context.CancellationToken);
    }
}
