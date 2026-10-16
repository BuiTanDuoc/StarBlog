using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// Scheduled every ReminderSettings.CronExpression (default: every 15 minutes)
/// via Hangfire — see RecurringJobsExtensions. Finds appointments entering
/// their reminder window since the last run, sends a reminder over every
/// channel the patient has contact info for and the tenant hasn't disabled,
/// and stamps Appointment.ReminderSentAt so the same appointment is never
/// reminded twice even if the job's window math overlaps between runs.
///
/// Window math: on each run we look for appointments starting between
/// (now + HoursBeforeAppointment) and (now + HoursBeforeAppointment + one
/// job interval). As long as CronExpression's interval isn't longer than
/// HoursBeforeAppointment allows for slack, every appointment passes through
/// this window exactly once. ReminderSentAt is still checked as a hard
/// guard in case the job runs twice for the same window (retries, overlap).
/// </summary>
public class AppointmentReminderJob
{
    private readonly ApplicationDbContext _context;
    private readonly ReminderSettings _settings;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly ILogger<AppointmentReminderJob> _logger;

    public AppointmentReminderJob(
        ApplicationDbContext context,
        IOptions<ReminderSettings> settings,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IWhatsAppSender whatsAppSender,
        ILogger<AppointmentReminderJob> logger)
    {
        _context = context;
        _settings = settings.Value;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _whatsAppSender = whatsAppSender;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        if (!_settings.Enabled)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var windowStart = now.AddHours(_settings.HoursBeforeAppointment);
        // A generous window (+ up to 2 hours past the exact reminder point) absorbs job
        // delays/retries without ever reminding an appointment twice, thanks to the
        // ReminderSentAt guard below.
        var windowEnd = windowStart.AddHours(2);

        // IgnoreQueryFilters: same reasoning as ExpiredSubscriptionsJob — this runs outside
        // any tenant's HTTP context and must see every tenant's appointments.
        var dueAppointments = await _context.Appointments
            .IgnoreQueryFilters()
            .Where(a => !a.IsDeleted
                        && a.ReminderSentAt == null
                        && a.ScheduledStart >= windowStart
                        && a.ScheduledStart < windowEnd
                        && (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .ToListAsync();

        if (dueAppointments.Count == 0)
        {
            return;
        }

        foreach (var appointment in dueAppointments)
        {
            await SendReminderAsync(appointment);
            appointment.ReminderSentAt = now;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("AppointmentReminderJob: sent {Count} reminder(s).", dueAppointments.Count);
    }

    private async Task SendReminderAsync(Appointment appointment)
    {
        var patient = appointment.Patient;
        var doctorName = appointment.Doctor?.FullName ?? "your doctor";
        var when = appointment.ScheduledStart.ToString("dddd, dd MMM yyyy 'at' HH:mm");
        var message = $"Reminder: you have an appointment with {doctorName} on {when}.";

        if (_settings.SendEmail && !string.IsNullOrWhiteSpace(patient.Email))
        {
            await _emailSender.SendAsync(patient.Email, "Appointment Reminder", message);
        }

        if (_settings.SendSms && !string.IsNullOrWhiteSpace(patient.Phone))
        {
            await _smsSender.SendAsync(patient.Phone, message);
        }

        if (_settings.SendWhatsApp && !string.IsNullOrWhiteSpace(patient.Phone))
        {
            await _whatsAppSender.SendAsync(patient.Phone, message);
        }

        // Also drop an in-app notification for the doctor's own dashboard, mirroring how
        // AppointmentEventHandlers notifies on create/cancel — not strictly a "reminder to
        // the patient", but keeps the doctor's notification feed showing what's imminent.
        if (appointment.Doctor?.ApplicationUserId is { } doctorUserId)
        {
            _context.Notifications.Add(new Notification
            {
                TenantId = appointment.TenantId,
                UserId = doctorUserId,
                Title = "Upcoming Appointment",
                Message = $"Appointment with {patient.FullName} is coming up on {when}.",
                Type = NotificationType.AppointmentReminder,
                RelatedEntityId = appointment.Id.ToString()
            });
        }
    }
}
