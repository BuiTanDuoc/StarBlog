namespace ClinicManagement.Infrastructure.Options;

/// <summary>
/// Bound from the "Twilio" section of appsettings.json. Used for both SMS and
/// WhatsApp — Twilio's REST API is identical for both, the only difference is
/// the "From" number is prefixed "whatsapp:" for WhatsApp messages. Calling
/// Twilio's REST endpoint directly with HttpClient (rather than adding the
/// Twilio NuGet package) keeps this feature buildable without a package
/// restore, since this environment can't reach nuget.org.
/// If AccountSid is empty, TwilioSmsSender/TwilioWhatsAppSender log instead
/// of sending — see the class comments there.
/// </summary>
public class TwilioSettings
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string SmsFromNumber { get; set; } = string.Empty;
    public string WhatsAppFromNumber { get; set; } = string.Empty;
}
