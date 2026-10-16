using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Coarse, fast HTTP-level gate: does this tenant have *any* valid
/// subscription at all? Runs after authentication (needs the JWT's
/// tenant_id claim) and before MVC. This is deliberately dumber than
/// ISubscriptionLimitService (Application layer, Phase 3) — it doesn't
/// know about per-resource quotas, it only asks "has the whole
/// subscription expired", which is cheap enough to check on every request.
/// </summary>
public class SubscriptionGuardMiddleware
{
    private static readonly string[] ExemptPathPrefixes =
    {
        "/api/auth", "/api/subscriptions", "/api/plans", "/api/webhooks", "/swagger", "/health", "/hangfire", "/hubs"
    };

    private readonly RequestDelegate _next;

    public SubscriptionGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        if (ShouldSkip(context, currentUserService))
        {
            await _next(context);
            return;
        }

        var hasValidSubscription = await dbContext.Subscriptions
            .IgnoreQueryFilters()
            .AnyAsync(s => s.TenantId == currentUserService.TenantId
                           && !s.IsDeleted
                           && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing)
                           && s.EndDate >= DateTime.UtcNow);

        if (!hasValidSubscription)
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                succeeded = false,
                errors = new[] { "Your clinic's subscription has expired or is inactive. Please renew your plan to continue." }
            });
            return;
        }

        await _next(context);
    }

    private static bool ShouldSkip(HttpContext context, ICurrentUserService currentUserService)
    {
        // Unauthenticated requests are for [Authorize] to reject with 401 — not this middleware's job.
        if (!currentUserService.IsAuthenticated) return true;

        // SuperAdmin manages the platform, not a single clinic's subscription.
        if (currentUserService.IsSuperAdmin) return true;

        var path = context.Request.Path.Value ?? string.Empty;
        return ExemptPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}

public static class SubscriptionGuardMiddlewareExtensions
{
    public static IApplicationBuilder UseSubscriptionGuard(this IApplicationBuilder app) =>
        app.UseMiddleware<SubscriptionGuardMiddleware>();
}
