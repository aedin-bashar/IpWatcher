using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;

namespace IpWatcher.Infrastructure.PublicIp;

public sealed class IpifyPublicIpProvider(HttpClient httpClient) : IPublicIpProvider
{
    public async Task<IpAddress> GetPublicIpAsync(CancellationToken cancellationToken)
    {
        var ipText = await httpClient.GetStringAsync("", cancellationToken).ConfigureAwait(false);
        return IpAddress.Parse(ipText);
    }
}
