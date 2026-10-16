namespace ClinicManagement.Domain.Common;

/// <summary>
/// Root marker for every entity in the system. Uses Guid keys so IDs can be
/// generated client-side (useful for offline-first flows and avoids leaking
/// row counts across tenants via sequential integer IDs).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Optimistic concurrency token (EF Core "rowversion" column).
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Adds creation/modification/soft-delete tracking. Populated automatically
/// by the SaveChanges interceptor in the Persistence layer — do not set
/// these manually from Application handlers.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Implemented by every entity that belongs to a single clinic (tenant).
/// The Persistence layer applies a global EF Core query filter on TenantId
/// for every type implementing this interface — see
/// ClinicManagement.Persistence/Extensions/ModelBuilderExtensions.cs (Phase 2).
/// </summary>
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}

/// <summary>
/// Convenience base class combining auditing + tenant isolation.
/// Nearly every business entity (Patient, Doctor, Appointment, Invoice, ...)
/// should derive from this rather than AuditableEntity directly.
/// </summary>
public abstract class TenantAuditableEntity : AuditableEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
}
