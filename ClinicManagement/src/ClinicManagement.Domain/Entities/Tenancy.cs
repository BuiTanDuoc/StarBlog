using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A subscribing clinic. This is the tenant root — every IMustHaveTenant
/// entity's TenantId points here. Resolved per-request from the "tenant_id"
/// JWT claim by ITenantService (Infrastructure, Phase 2).
/// </summary>
public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;

    /// <summary>URL-safe unique identifier, e.g. used as subdomain: {slug}.clinicsaas.com</summary>
    public string Slug { get; set; } = default!;

    public string ContactEmail { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

/// <summary>
/// A billing plan (Free/Starter/Professional/Enterprise). Global catalog —
/// not tenant-scoped, managed by SuperAdmin only.
/// </summary>
public class Plan : AuditableEntity
{
    public string Name { get; set; } = default!;
    public PlanTier Tier { get; set; }
    public decimal Price { get; set; }
    public int DurationInDays { get; set; } = 30;

    public int MaxDoctors { get; set; }
    public int MaxEmployees { get; set; }
    public int MaxPatients { get; set; }
    public int MaxAppointmentsPerMonth { get; set; }

    /// <summary>Storage limit in megabytes, enforced by the file upload pipeline.</summary>
    public long StorageLimitMb { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

/// <summary>
/// A tenant's subscription to a plan for a period. SubscriptionGuardMiddleware
/// (API layer, Phase 5) reads the active Subscription for the current tenant
/// on every mutating request to enforce plan limits.
/// </summary>
public class Subscription : TenantAuditableEntity
{
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = default!;
    public Tenant Tenant { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trialing;
    public bool AutoRenew { get; set; }

    public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
}

/// <summary>A single payment made towards a Subscription (distinct from clinic-to-patient billing).</summary>
public class SubscriptionPayment : TenantAuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }
}
