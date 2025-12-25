using IpWatcher.Application.Abstractions;

namespace IpWatcher.Application.UseCases;

public sealed class CheckIpChangeUseCase(
    IPublicIpProvider publicIpProvider,
    IIpStorage ipStorage,
    IEmailNotifier emailNotifier)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var currentIp = await publicIpProvider.GetPublicIpAsync(cancellationToken).ConfigureAwait(false);
        var previousIp = await ipStorage.LoadLastIpAsync(cancellationToken).ConfigureAwait(false);

        if (previousIp is null)
        {
            await emailNotifier.NotifyIpChangedAsync(null, currentIp, cancellationToken).ConfigureAwait(false);
            await ipStorage.SaveLastIpAsync(currentIp, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (previousIp == currentIp)
        {
            return;
        }

        await emailNotifier.NotifyIpChangedAsync(previousIp, currentIp, cancellationToken).ConfigureAwait(false);
        await ipStorage.SaveLastIpAsync(currentIp, cancellationToken).ConfigureAwait(false);
    }
}
