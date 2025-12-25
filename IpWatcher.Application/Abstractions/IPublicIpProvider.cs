using IpWatcher.Domain.ValueObjects;

namespace IpWatcher.Application.Abstractions;

public interface IPublicIpProvider
{
    Task<IpAddress> GetPublicIpAsync(CancellationToken cancellationToken);
}
