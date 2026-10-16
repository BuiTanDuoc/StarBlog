namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Resolves "who is making this request" from the current HTTP context's JWT
/// claims. Implemented in Infrastructure (Phase 4) via IHttpContextAccessor.
/// Consumed by: the Persistence global tenant query filter, the audit
/// interceptor, and any handler that needs to know the caller's identity.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    /// <summary>The tenant_id claim. Guid.Empty for SuperAdmin (who isn't scoped to any single clinic).</summary>
    Guid TenantId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    /// <summary>True only for the SuperAdmin role — bypasses tenant query filters platform-wide.</summary>
    bool IsSuperAdmin { get; }

    IReadOnlyList<string> Roles { get; }

    /// <summary>The "patient_id" claim, present only on tokens issued to a patient-portal login (see TokenService). Null for every staff/SuperAdmin token.</summary>
    Guid? PatientId { get; }
}
