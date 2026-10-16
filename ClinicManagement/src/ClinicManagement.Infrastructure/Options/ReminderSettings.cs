namespace ClinicManagement.Infrastructure.Options;

/// <summary>Bound from the "Reminders" section of appsettings.json.</summary>
public class ReminderSettings
{
    public const string SectionName = "Reminders";

    public bool Enabled { get; set; } = true;

    /// <summary>How long before ScheduledStart a reminder goes out.</summary>
    public int HoursBeforeAppointment { get; set; } = 24;

    /// <summary>
    /// How often AppointmentReminderJob runs (cron). Must be frequent enough, relative to
    /// HoursBeforeAppointment, that no appointment's reminder window is skipped entirely
    /// between two runs — see the comment in AppointmentReminderJob for the window math.
    /// </summary>
    public string CronExpression { get; set; } = "*/15 * * * *";

    public bool SendEmail { get; set; } = true;
    public bool SendSms { get; set; } = true;
    public bool SendWhatsApp { get; set; } = false;
}
