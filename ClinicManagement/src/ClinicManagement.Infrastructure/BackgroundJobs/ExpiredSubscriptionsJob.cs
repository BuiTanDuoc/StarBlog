using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using ClinicManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// Scheduled hourly via Hangfire (see RecurringJobsExtensions,
/// wired up from API/Program.cs in Phase 6). Resolved fresh from DI on each
/// run, so it gets its own scoped ApplicationDbContext per execution — no
/// stale-context issues from long-lived background threads.
/// </summary>
public class ExpiredSubscriptionsJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpiredSubscriptionsJob> _logger;

    public ExpiredSubscriptionsJob(ApplicationDbContext context, ILogger<ExpiredSubscriptionsJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;

        // IgnoreQueryFilters: this job runs outside any single tenant's context and must see
        // every tenant's subscriptions, not just one — the opposite of every other query in
        // this codebase, which is exactly why it's confined to this one background job.
        var expiredSubscriptions = await _context.Subscriptions
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted
                        && s.EndDate < now
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .ToListAsync();

        if (expiredSubscriptions.Count == 0)
        {
            _logger.LogInformation("ExpiredSubscriptionsJob: no subscriptions to expire.");
            return;
        }

        foreach (var subscription in expiredSubscriptions)
        {
            subscription.Status = SubscriptionStatus.Expired;
            await NotifyClinicOwnerAsync(subscription);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("ExpiredSubscriptionsJob: expired {Count} subscription(s).", expiredSubscriptions.Count);
    }

    /// <summary>
    /// Queries the DbContext directly (rather than UserManager) with
    /// IgnoreQueryFilters explicitly scoped to this one lookup. UserManager
    /// would go through the same tenant query filter as everything else —
    /// and since this job has no HTTP request/JWT to resolve a "current
    /// tenant" from, that filter would silently match nothing.
    /// </summary>
    private async Task NotifyClinicOwnerAsync(Subscription subscription)
    {
        var ownerRoleId = await _context.Roles
            .Where(r => r.Name == Roles.ClinicOwner)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        if (ownerRoleId == Guid.Empty) return;

        var ownerUserIds = await _context.UserRoles
            .Where(ur => ur.RoleId == ownerRoleId)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var tenantOwners = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => ownerUserIds.Contains(u.Id) && u.TenantId == subscription.TenantId)
            .ToListAsync();

        foreach (var owner in tenantOwners)
        {
            _context.Notifications.Add(new Notification
            {
                TenantId = subscription.TenantId,
                UserId = owner.Id,
                Title = "Subscription Expired",
                Message = "Your clinic's subscription has expired. Renew your plan to restore full access.",
                Type = NotificationType.SubscriptionExpired,
                RelatedEntityId = subscription.Id.ToString()
            });
        }
    }
}
