using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicalRecord : TenantAuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public string Diagnosis { get; set; } = default!;
    public string? Symptoms { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }

    /// <summary>Optional specialty template this record was filled in against (dental, ophthalmology, etc.) — see MedicalRecordTemplate.</summary>
    public Guid? TemplateId { get; set; }
    public MedicalRecordTemplate? Template { get; set; }

    /// <summary>Serialized {fieldKey: value} answers matching Template.FieldsSchemaJson. Null when no template was used (a plain free-text record).</summary>
    public string? CustomFieldsJson { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<FileAttachment> Attachments { get; set; } = new List<FileAttachment>();
}

/// <summary>
/// A clinic-configurable exam form for a given specialization (e.g. "Dental
/// Exam", "Eye Exam") — an alternative to the generic free-text Diagnosis/
/// Symptoms/Notes fields on MedicalRecord for doctors who want structured,
/// specialty-specific fields (tooth chart, visual acuity, etc.). FieldsSchemaJson
/// is a small JSON array the frontend renders as a dynamic form, e.g.:
/// [{"key":"visualAcuityLeft","label":"Visual Acuity (Left)","type":"text"},
///  {"key":"intraocularPressure","label":"IOP (mmHg)","type":"number"}]
/// Kept as JSON rather than a rigid table-per-specialty so a clinic owner can
/// define new templates without a schema migration for every specialty.
/// </summary>
public class MedicalRecordTemplate : TenantAuditableEntity
{
    public string Name { get; set; } = default!;

    /// <summary>Free-text match against Doctor.Specialization (also free-text) — intentionally loose, same reasoning as elsewhere in this codebase.</summary>
    public string Specialization { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public string FieldsSchemaJson { get; set; } = "[]";

    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}

public class Prescription : TenantAuditableEntity
{
    public Guid MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = default!;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = default!;

    public DateTime DateIssued { get; set; } = DateTime.UtcNow;

    /// <summary>Set once the PDF has been generated; served from FileAttachment or blob storage.</summary>
    public string? GeneratedPdfUrl { get; set; }

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}

public class PrescriptionItem : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = default!;

    public string MedicineName { get; set; } = default!;
    public string Dosage { get; set; } = default!;
    public string Frequency { get; set; } = default!;
    public string Duration { get; set; } = default!;
    public string? Instructions { get; set; }
}
