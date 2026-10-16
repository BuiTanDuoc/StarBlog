using System.Net.Http.Headers;
using System.Text;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Notifications;

/// <summary>
/// Calls Twilio's REST API (https://www.twilio.com/docs/sms/send-messages)
/// directly via HttpClient instead of the Twilio SDK NuGet package, so this
/// compiles without a package restore in environments (like this one) that
/// can't reach nuget.org. Swap this out for Vonage/etc. by implementing
/// ISmsSender the same way if Twilio isn't the provider of choice.
/// </summary>
public class TwilioSmsSender : ISmsSender
{
    private const string ApiBase = "https://api.twilio.com/2010-04-01/Accounts";

    private readonly TwilioSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(IOptions<TwilioSettings> settings, IHttpClientFactory httpClientFactory, ILogger<TwilioSmsSender> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(string toPhoneE164, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.AccountSid))
        {
            _logger.LogInformation("Twilio not configured — SMS to {ToPhone} not sent: {Message}", toPhoneE164, message);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(TwilioSmsSender));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.AccountSid}:{_settings.AuthToken}")));

            var form = new Dictionary<string, string>
            {
                ["To"] = toPhoneE164,
                ["From"] = _settings.SmsFromNumber,
                ["Body"] = message
            };

            var response = await client.PostAsync(
                $"{ApiBase}/{_settings.AccountSid}/Messages.json", new FormUrlEncodedContent(form), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Twilio SMS to {ToPhone} failed ({Status}): {Body}", toPhoneE164, response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send reminder SMS to {ToPhone}", toPhoneE164);
        }
    }
}
