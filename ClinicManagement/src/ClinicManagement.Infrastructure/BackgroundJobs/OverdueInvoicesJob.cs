using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using ClinicManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// Scheduled daily via Hangfire (see RecurringJobsExtensions). Finds
/// invoices past their DueDate that still have a balance owing, flips them
/// to InvoiceStatus.Overdue (previously nothing in the codebase ever set
/// that status — invoices just sat at Issued/PartiallyPaid forever, however
/// late), reminds the patient over the same channels as
/// AppointmentReminderJob, and notifies clinic staff who handle billing.
/// </summary>
public class OverdueInvoicesJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ILogger<OverdueInvoicesJob> _logger;

    public OverdueInvoicesJob(
        ApplicationDbContext context, IEmailSender emailSender, ISmsSender smsSender, ILogger<OverdueInvoicesJob> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;

        // IgnoreQueryFilters: same reasoning as ExpiredSubscriptionsJob — runs outside any
        // tenant's HTTP context and must see every tenant's invoices.
        var candidates = await _context.Invoices
            .IgnoreQueryFilters()
            .Where(i => !i.IsDeleted
                        && i.DueDate < now
                        && (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid))
            .Include(i => i.Patient)
            .Include(i => i.Payments)
            .ToListAsync();

        var overdue = candidates.Where(i => i.Total - i.Payments.Sum(p => p.Amount) > 0).ToList();
        if (overdue.Count == 0)
        {
            _logger.LogInformation("OverdueInvoicesJob: no invoices to flag.");
            return;
        }

        foreach (var invoice in overdue)
        {
            invoice.Status = InvoiceStatus.Overdue;
            await RemindPatientAsync(invoice);
            await NotifyBillingStaffAsync(invoice);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("OverdueInvoicesJob: flagged {Count} invoice(s) as overdue.", overdue.Count);
    }

    private async Task RemindPatientAsync(Invoice invoice)
    {
        var balance = invoice.Total - invoice.Payments.Sum(p => p.Amount);
        var message = $"Your invoice {invoice.InvoiceNumber} for {balance:C} was due on {invoice.DueDate:d} and is now overdue. Please arrange payment at your earliest convenience.";

        if (!string.IsNullOrWhiteSpace(invoice.Patient.Email))
        {
            await _emailSender.SendAsync(invoice.Patient.Email, "Overdue Invoice", message);
        }
        if (!string.IsNullOrWhiteSpace(invoice.Patient.Phone))
        {
            await _smsSender.SendAsync(invoice.Patient.Phone, message);
        }
    }

    /// <summary>Same UserManager-avoidance reasoning as ExpiredSubscriptionsJob.NotifyClinicOwnerAsync — this job has no ambient tenant to filter through.</summary>
    private async Task NotifyBillingStaffAsync(Invoice invoice)
    {
        var billingRoleNames = new[] { Roles.ClinicOwner, Roles.Accountant };
        var billingRoleIds = await _context.Roles
            .Where(r => billingRoleNames.Contains(r.Name))
            .Select(r => r.Id)
            .ToListAsync();

        if (billingRoleIds.Count == 0) return;

        var billingUserIds = await _context.UserRoles
            .Where(ur => billingRoleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .ToListAsync();

        var recipients = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => billingUserIds.Contains(u.Id) && u.TenantId == invoice.TenantId)
            .ToListAsync();

        foreach (var recipient in recipients)
        {
            _context.Notifications.Add(new Notification
            {
                TenantId = invoice.TenantId,
                UserId = recipient.Id,
                Title = "Invoice Overdue",
                Message = $"Invoice {invoice.InvoiceNumber} is now overdue.",
                Type = NotificationType.General,
                RelatedEntityId = invoice.Id.ToString()
            });
        }
    }
}
