using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// A user-facing notification, pushed in real time over SignalR
/// (NotificationHub, API layer, Phase 6) and persisted so it survives
/// disconnects and shows up in a notification inbox.
/// </summary>
public class Notification : TenantAuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    /// <summary>Optional pointer back to the entity that triggered this notification (e.g. an AppointmentId).</summary>
    public string? RelatedEntityId { get; set; }
}

/// <summary>
/// Immutable record of who changed what. Written by a SaveChanges
/// interceptor (Persistence, Phase 2) so handlers never need to log audit
/// entries manually. Old/NewValues are serialized JSON snapshots.
/// </summary>
public class AuditLog : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}

/// <summary>
/// Metadata for a file stored out-of-band (disk/S3/Azure Blob — see
/// IFileStorageService, Infrastructure Phase 2). Never stores file bytes
/// in the database.
/// </summary>
public class FileAttachment : TenantAuditableEntity
{
    public string FileName { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }

    /// <summary>Name of the owning entity type, e.g. "MedicalRecord", "Patient".</summary>
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }

    public string? UploadedBy { get; set; }

    public Guid? MedicalRecordId { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }
}
