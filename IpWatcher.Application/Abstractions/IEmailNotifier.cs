using IpWatcher.Domain.ValueObjects;

namespace IpWatcher.Application.Abstractions;

public interface IEmailNotifier
{
    Task NotifyIpChangedAsync(IpAddress? previousIp, IpAddress currentIp, CancellationToken cancellationToken);
}
