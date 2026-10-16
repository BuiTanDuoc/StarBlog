using System.Net.Http.Headers;
using System.Text;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Notifications;

/// <summary>
/// Twilio's WhatsApp Business API rides the same /Messages.json endpoint as
/// SMS — the only difference is both To and From are prefixed "whatsapp:".
/// See TwilioSmsSender for why this calls the REST API directly instead of
/// using a NuGet SDK.
/// </summary>
public class TwilioWhatsAppSender : IWhatsAppSender
{
    private const string ApiBase = "https://api.twilio.com/2010-04-01/Accounts";

    private readonly TwilioSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TwilioWhatsAppSender> _logger;

    public TwilioWhatsAppSender(IOptions<TwilioSettings> settings, IHttpClientFactory httpClientFactory, ILogger<TwilioWhatsAppSender> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string toPhoneE164, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AccountSid) || string.IsNullOrWhiteSpace(_settings.WhatsAppFromNumber))
        {
            _logger.LogInformation("Twilio WhatsApp not configured — message to {ToPhone} not sent: {Message}", toPhoneE164, message);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(TwilioWhatsAppSender));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}")));

            var form = new Dictionary<string, string>
            {
                ["To"] = $"whatsapp:{toPhoneE164}",
                ["From"] = $"whatsapp:{_settings.WhatsAppFromNumber}",
                ["Body"] = message
            };

            var response = await client.PostAsync(
                $"{ApiBase}/{_settings.AccountSid}/Messages.json", new FormUrlEncodedContent(form), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Twilio WhatsApp to {ToPhone} failed ({Status}): {Body}", toPhoneE164, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send reminder WhatsApp message to {ToPhone}", toPhoneE164);
        }
    }
}
