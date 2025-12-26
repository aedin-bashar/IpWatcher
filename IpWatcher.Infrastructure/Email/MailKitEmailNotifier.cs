using IpWatcher.Application.Abstractions;
using IpWatcher.Domain.ValueObjects;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IpWatcher.Infrastructure.Email;

public sealed class MailKitEmailNotifier(IOptions<EmailOptions> options, ILogger<MailKitEmailNotifier> logger) : IEmailNotifier
{
    private readonly EmailOptions _options = options.Value;

    public async Task NotifyIpChangedAsync(IpAddress? previousIp, IpAddress currentIp, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Sending IP change email. Previous={PreviousIp}, Current={CurrentIp}, Host={Host}:{Port}, UseSsl={UseSsl}.",
            previousIp?.Value,
            currentIp.Value,
            _options.Host,
            _options.Port,
            _options.UseSsl);

        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_options.From));
            message.To.Add(MailboxAddress.Parse(_options.To));
            message.Subject = $"{_options.SubjectPrefix} IP changed to {currentIp.Value}";
            message.Body = new TextPart("plain")
            {
                Text = previousIp is null
                    ? $"Public IP observed: {currentIp.Value}"
                    : $"Public IP changed from {previousIp.Value} to {currentIp.Value}"
            };

            using var smtp = new SmtpClient();

            var socketOptions =
                _options.UseSsl
                    ? (_options.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls)
                    : SecureSocketOptions.Auto;

            await smtp.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(_options.UserName))
                await smtp.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken).ConfigureAwait(false);

            await smtp.SendAsync(message, cancellationToken).ConfigureAwait(false);
            await smtp.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            logger.LogInformation("IP change email sent successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send IP change email.");
            throw;
        }
    }
}
