using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Persistence.Seed;

/// <summary>
/// Orchestrates all seed steps in the right order. Call
/// `await app.Services.SeedDatabaseAsync()` once at API startup (Phase 6,
/// after `context.Database.MigrateAsync()`). Idempotent — safe to run on
/// every deployment.
/// </summary>
public static class DatabaseSeeder
{
    public const string SuperAdminEmail = "superadmin@clinicsaas.dev";
    public const string DemoTenantSlug = "demo-clinic";
    public const string DemoOwnerEmail = "owner@demo-clinic.dev";

    /// <summary>Default password for seeded accounts in local/dev environments only — never used in production seeding.</summary>
    private const string DevPassword = "ChangeMe!123";

    public static async Task SeedDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        await RoleSeeder.SeedAsync(roleManager);
        await PlanSeeder.SeedAsync(context);
        await SeedSuperAdminAsync(userManager);
        await SeedDemoTenantAsync(context, userManager);
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync(SuperAdminEmail) is not null) return;

        var superAdmin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.Empty, // platform-level account, not scoped to any clinic
            UserName = SuperAdminEmail,
            Email = SuperAdminEmail,
            EmailConfirmed = true,
            FullName = "Platform Super Admin",
            IsActive = true
        };

        var result = await userManager.CreateAsync(superAdmin, DevPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
        }
    }

    private static async Task SeedDemoTenantAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var existingTenant = await context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Slug == DemoTenantSlug);
        if (existingTenant is not null) return;

        var freePlan = await context.Plans.IgnoreQueryFilters()
            .FirstAsync(p => p.Tier == PlanTier.Free);

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Demo Clinic",
            Slug = DemoTenantSlug,
            ContactEmail = DemoOwnerEmail,
            IsActive = true
        };
        await context.Tenants.AddAsync(tenant);

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            PlanId = freePlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(freePlan.DurationInDays),
            Status = SubscriptionStatus.Trialing,
            AutoRenew = false
        };
        await context.Subscriptions.AddAsync(subscription);

        // SaveChanges here (rather than deferring to the caller) so the tenant exists
        // before we create its owner user, which needs a valid TenantId FK.
        await context.SaveChangesAsync();

        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            UserName = DemoOwnerEmail,
            Email = DemoOwnerEmail,
            EmailConfirmed = true,
            FullName = "Demo Clinic Owner",
            IsActive = true
        };

        var result = await userManager.CreateAsync(owner, DevPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(owner, Roles.ClinicOwner);
        }
    }
}
