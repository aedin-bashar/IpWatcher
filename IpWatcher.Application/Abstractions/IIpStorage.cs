using IpWatcher.Domain.ValueObjects;

namespace IpWatcher.Application.Abstractions;

public interface IIpStorage
{
    Task<IpAddress?> LoadLastIpAsync(CancellationToken cancellationToken);
    Task SaveLastIpAsync(IpAddress ipAddress, CancellationToken cancellationToken);
}
