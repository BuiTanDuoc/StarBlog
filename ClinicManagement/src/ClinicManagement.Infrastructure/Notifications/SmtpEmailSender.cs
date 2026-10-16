using System.Net;
using System.Net.Mail;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Notifications;

/// <summary>
/// Uses System.Net.Mail rather than a third-party mail package, so this
/// compiles and runs with zero extra NuGet restores. If Smtp:Host isn't
/// configured (the default in appsettings.json), this logs the message
/// instead of throwing — an unconfigured reminder channel should never
/// crash the appointment-reminder job or block clinic signup/booking.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogInformation("Smtp not configured — email to {ToEmail} not sent. Subject: {Subject}", toEmail, subject);
            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // A reminder that fails to send is a degraded experience, not a system failure —
            // logging and swallowing keeps one bad email address from blocking the whole
            // recurring job's batch. See the same reasoning in TwilioSmsSender/WhatsAppSender.
            _logger.LogWarning(ex, "Failed to send reminder email to {ToEmail}", toEmail);
        }
    }
}
