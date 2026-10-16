namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// One interface per external channel rather than a single "send notification"
/// abstraction — SMS/WhatsApp/Email have genuinely different addressing
/// (phone vs. email) and payload shape (plain text vs. subject+body), and
/// callers usually want to fan out to whichever channels the patient has
/// contact info for rather than picking exactly one.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}

public interface ISmsSender
{
    Task SendAsync(string toPhoneE164, string message, CancellationToken cancellationToken = default);
}

public interface IWhatsAppSender
{
    Task SendAsync(string toPhoneE164, string message, CancellationToken cancellationToken = default);
}
