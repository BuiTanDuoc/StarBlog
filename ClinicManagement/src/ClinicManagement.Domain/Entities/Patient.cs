using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : TenantAuditableEntity
{
    /// <summary>Human-friendly sequential code per tenant, e.g. "PT-000123". Generated in Application layer.</summary>
    public string PatientCode { get; set; } = default!;

    public string? NationalId { get; set; }
    public string FullName { get; set; } = default!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = default!;

    /// <summary>Used for email appointment reminders and, optionally, the patient portal login. Nullable — many patients are phone-only.</summary>
    public string? Email { get; set; }

    /// <summary>Set once this patient claims/creates a patient-portal login (see RegisterPatientAccountCommand). Null for patients who only exist in-clinic.</summary>
    public Guid? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public string? Address { get; set; }
    public BloodGroup BloodGroup { get; set; } = BloodGroup.Unknown;

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }

    public string? InsuranceProvider { get; set; }
    public string? InsuranceNumber { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
