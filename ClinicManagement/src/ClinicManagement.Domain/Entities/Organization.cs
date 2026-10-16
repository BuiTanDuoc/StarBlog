using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class Branch : TenantAuditableEntity
{
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

public class Department : TenantAuditableEntity
{
    public string Name { get; set; } = default!;

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}

public class Doctor : TenantAuditableEntity
{
    /// <summary>Nullable: a doctor record can exist before the linked login account is created.</summary>
    public Guid? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string FullName { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public string LicenseNumber { get; set; } = default!;
    public decimal ConsultationFee { get; set; }

    /// <summary>Serialized JSON: [{ "Day": "Monday", "Start": "09:00", "End": "17:00" }, ...]</summary>
    public string WorkScheduleJson { get; set; } = "[]";

    public int YearsOfExperience { get; set; }
    public DoctorStatus Status { get; set; } = DoctorStatus.Active;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}

public class Employee : TenantAuditableEntity
{
    public Guid? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string FullName { get; set; } = default!;
    public string Position { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
}
