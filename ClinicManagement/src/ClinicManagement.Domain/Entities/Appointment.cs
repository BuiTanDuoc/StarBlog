using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : TenantAuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    /// <summary>Set by AppointmentReminderJob once a reminder (SMS/WhatsApp/Email) has gone out, so the same appointment is never reminded twice.</summary>
    public DateTime? ReminderSentAt { get; set; }

    /// <summary>Set when this visit redeems one session from a prepaid PatientPackage instead of billing separately — see CreateAppointmentCommand and CancelAppointmentCommand.</summary>
    public Guid? PatientPackageId { get; set; }
    public PatientPackage? PatientPackage { get; set; }

    public ICollection<AppointmentStatusHistory> StatusHistory { get; set; } = new List<AppointmentStatusHistory>();
    public MedicalRecord? MedicalRecord { get; set; }
    public Invoice? Invoice { get; set; }
}

/// <summary>
/// Immutable audit trail of every status transition an appointment goes
/// through (Pending → Confirmed → CheckedIn → InProgress → Completed, or a
/// Cancelled/NoShow branch). Written by the same handler that changes
/// Appointment.Status — never edited afterwards.
/// </summary>
public class AppointmentStatusHistory : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = default!;

    public AppointmentStatus FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}
