using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace IpWatcher.Infrastructure.Storage;

public sealed class FileIpStorage(IOptions<IpStorageOptions> options) : IIpStorage
{
    private readonly IpStorageOptions _options = options.Value;

    public async Task<IpAddress?> LoadLastIpAsync(CancellationToken cancellationToken)
    {
        var path = _options.FilePath;
        if (!File.Exists(path))
        {
            return null;
        }

        var contents = (await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false)).Trim();
        return string.IsNullOrWhiteSpace(contents) ? null : IpAddress.Parse(contents);
    }

    public async Task SaveLastIpAsync(IpAddress ipAddress, CancellationToken cancellationToken)
    {
        var path = _options.FilePath;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(path, ipAddress.Value + Environment.NewLine, cancellationToken).ConfigureAwait(false);
    }
}
