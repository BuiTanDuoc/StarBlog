namespace ClinicManagement.Infrastructure.Options;

/// <summary>Bound from the "Smtp" section of appsettings.json. If Host is empty, SmtpEmailSender logs instead of sending — see the class comment there.</summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@clinic.local";
    public string FromName { get; set; } = "Clinic Management";
}
