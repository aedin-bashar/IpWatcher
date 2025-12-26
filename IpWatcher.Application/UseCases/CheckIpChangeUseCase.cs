using IpWatcher.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace IpWatcher.Application.UseCases;

public sealed class CheckIpChangeUseCase(
    IPublicIpProvider publicIpProvider,
    IIpStorage ipStorage,
    IEmailNotifier emailNotifier,
    ILogger<CheckIpChangeUseCase> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Checking public IP...");

        var currentIp = await publicIpProvider.GetPublicIpAsync(cancellationToken).ConfigureAwait(false);
        var previousIp = await ipStorage.LoadLastIpAsync(cancellationToken).ConfigureAwait(false);

        if (previousIp is null)
        {
            logger.LogInformation("No previous IP found. Current IP is {CurrentIp}. Notifying and saving.", currentIp.Value);
            await emailNotifier.NotifyIpChangedAsync(null, currentIp, cancellationToken).ConfigureAwait(false);
            await ipStorage.SaveLastIpAsync(currentIp, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (previousIp == currentIp)
        {
            logger.LogInformation("Public IP unchanged ({CurrentIp}).", currentIp.Value);
            return;
        }

        logger.LogWarning("Public IP changed from {PreviousIp} to {CurrentIp}. Notifying and saving.", previousIp.Value, currentIp.Value);
        await emailNotifier.NotifyIpChangedAsync(previousIp, currentIp, cancellationToken).ConfigureAwait(false);
        await ipStorage.SaveLastIpAsync(currentIp, cancellationToken).ConfigureAwait(false);
    }
}
