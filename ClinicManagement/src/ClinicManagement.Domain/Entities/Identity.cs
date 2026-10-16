using ClinicManagement.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Extends ASP.NET Identity's IdentityUser with a Guid key and tenant
/// linkage. Every user (including SuperAdmin, whose TenantId is Guid.Empty
/// and who bypasses the tenant query filter — see Persistence Phase 2)
/// belongs to exactly one tenant.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;

    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // 1:1 optional links to role-specific profile records.
    public Doctor? DoctorProfile { get; set; }
    public Employee? EmployeeProfile { get; set; }
    public Patient? PatientProfile { get; set; }
}

/// <summary>
/// Opaque refresh token issued alongside a short-lived JWT access token.
/// Rotated on each use (ReplacedByToken) so a leaked token can be detected
/// and the whole chain revoked.
/// </summary>
public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = default!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt is null && !IsExpired;
}
