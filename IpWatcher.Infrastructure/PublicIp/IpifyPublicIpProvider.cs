using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace IpWatcher.Infrastructure.PublicIp;

public sealed class IpifyPublicIpProvider(HttpClient httpClient, ILogger<IpifyPublicIpProvider> logger) : IPublicIpProvider
{
    public async Task<IpAddress> GetPublicIpAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting public IP from {BaseAddress}.", httpClient.BaseAddress);

        var ipText = await httpClient.GetStringAsync("", cancellationToken).ConfigureAwait(false);
        var ip = IpAddress.Parse(ipText);

        logger.LogDebug("Public IP provider returned {CurrentIp}.", ip.Value);
        return ip;
    }
}
