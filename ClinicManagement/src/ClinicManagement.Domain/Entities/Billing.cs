using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Invoice : TenantAuditableEntity
{
    /// <summary>Human-friendly sequential number per tenant, e.g. "INV-2026-000482".</summary>
    public string InvoiceNumber { get; set; } = default!;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public decimal AmountPaid => Payments.Sum(p => p.Amount);
    public decimal Balance => Total - AmountPaid;
}

public class InvoiceItem : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public string Description { get; set; } = default!;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class Payment : TenantAuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }
    public string? ReceivedBy { get; set; }
}

public class Expense : TenantAuditableEntity
{
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public string Category { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; }
    public string? ApprovedBy { get; set; }
}

/// <summary>
/// A clinic's sellable session bundle — e.g. "10-Session Physiotherapy
/// Package" — sold once as a catalog item, then purchased per-patient as a
/// PatientPackage. Kept separate from Invoice/InvoiceItem: those model a
/// single bill, this models a reusable product a clinic defines once and
/// sells many times.
/// </summary>
public class TreatmentPackage : TenantAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int SessionsIncluded { get; set; }
    public decimal Price { get; set; }

    /// <summary>Sessions must be used within this many days of purchase — see PatientPackage.ExpiresAt.</summary>
    public int ValidityDays { get; set; } = 365;

    public bool IsActive { get; set; } = true;

    public ICollection<PatientPackage> PatientPackages { get; set; } = new List<PatientPackage>();
}

/// <summary>
/// One patient's purchased instance of a TreatmentPackage — "Jane bought the
/// 10-session package on Jan 1st, has used 3, expires Jan 1st next year".
/// InvoiceId points at the upfront invoice raised for the full package price;
/// individual sessions redeemed against it don't get their own invoice (see
/// CreateAppointmentCommand's PatientPackageId handling) since the patient
/// already paid for them as a bundle.
/// </summary>
public class PatientPackage : TenantAuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    public Guid TreatmentPackageId { get; set; }
    public TreatmentPackage TreatmentPackage { get; set; } = default!;

    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int SessionsTotal { get; set; }
    public int SessionsUsed { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public int SessionsRemaining => SessionsTotal - SessionsUsed;
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
