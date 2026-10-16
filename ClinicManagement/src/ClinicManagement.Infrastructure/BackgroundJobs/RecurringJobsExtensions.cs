using ClinicManagement.Infrastructure.Options;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.BackgroundJobs;

public static class RecurringJobsExtensions
{
    /// <summary>Call once at API startup, after app.UseHangfireDashboard(). Idempotent — AddOrUpdate is safe to call on every boot.</summary>
    public static void ScheduleRecurringJobs(this IRecurringJobManager recurringJobManager, IServiceProvider services)
    {
        recurringJobManager.AddOrUpdate<ExpiredSubscriptionsJob>(
            "deactivate-expired-subscriptions",
            job => job.ExecuteAsync(),
            Cron.Hourly);

        var reminderSettings = services.GetRequiredService<IOptions<ReminderSettings>>().Value;
        recurringJobManager.AddOrUpdate<AppointmentReminderJob>(
            "send-appointment-reminders",
            job => job.ExecuteAsync(),
            reminderSettings.CronExpression);

        recurringJobManager.AddOrUpdate<OverdueInvoicesJob>(
            "flag-overdue-invoices",
            job => job.ExecuteAsync(),
            Cron.Daily);
    }
}
