namespace ClinicManagement.Shared.Constants;

/// <summary>Seeded ASP.NET Identity roles. See Persistence/Seed/RoleSeeder (Phase 2).</summary>
public static class Roles
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string ClinicOwner = nameof(ClinicOwner);
    public const string Doctor = nameof(Doctor);
    public const string Receptionist = nameof(Receptionist);
    public const string Accountant = nameof(Accountant);
    public const string Nurse = nameof(Nurse);

    /// <summary>Self-service patient-portal login — see PatientPortalController. Scoped entirely to "own records" permissions, never the staff ones above.</summary>
    public const string Patient = nameof(Patient);

    public static readonly string[] All =
    {
        SuperAdmin, ClinicOwner, Doctor, Receptionist, Accountant, Nurse, Patient
    };
}

/// <summary>
/// Fine-grained permission strings used with a custom [HasPermission] policy
/// (Phase 4), layered on top of role-based authorization for finer control
/// (e.g. a Receptionist granted billing access without the full Accountant role).
/// </summary>
public static class Permissions
{
    public static class Patients
    {
        public const string View = "Patients.View";
        public const string Create = "Patients.Create";
        public const string Edit = "Patients.Edit";
        public const string Delete = "Patients.Delete";
    }

    public static class Appointments
    {
        public const string View = "Appointments.View";
        public const string Create = "Appointments.Create";
        public const string Edit = "Appointments.Edit";
        public const string Cancel = "Appointments.Cancel";
    }

    public static class Billing
    {
        public const string View = "Billing.View";
        public const string ManageInvoices = "Billing.ManageInvoices";
        public const string ManagePayments = "Billing.ManagePayments";
        public const string ManageExpenses = "Billing.ManageExpenses";

        /// <summary>Define the catalog of sellable session bundles (TreatmentPackage) — a clinic-configuration action, distinct from selling one to a specific patient.</summary>
        public const string ManageTreatmentPackages = "Billing.ManageTreatmentPackages";

        /// <summary>Sell a catalog TreatmentPackage to a specific patient (creates a PatientPackage + its upfront Invoice).</summary>
        public const string SellTreatmentPackages = "Billing.SellTreatmentPackages";
    }

    public static class MedicalRecords
    {
        public const string View = "MedicalRecords.View";
        public const string Create = "MedicalRecords.Create";
        public const string Edit = "MedicalRecords.Edit";

        /// <summary>Create/update/delete specialty exam templates (MedicalRecordTemplate) — a clinic-configuration action, not a per-patient one.</summary>
        public const string ManageTemplates = "MedicalRecords.ManageTemplates";
    }

    public static class Tenants
    {
        public const string Manage = "Tenants.Manage";
        public const string ManageSubscriptions = "Tenants.ManageSubscriptions";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    /// <summary>
    /// Patient-portal permissions. Deliberately separate from the staff Permissions above rather
    /// than reusing e.g. Appointments.View — a patient's JWT must never satisfy a staff-only
    /// policy, even by accident, because staff endpoints return every patient's data for the
    /// tenant while portal endpoints filter to the caller's own PatientId. Keeping the string
    /// namespaces distinct means a handler that forgets to filter fails closed (wrong policy),
    /// not open (right policy, wrong result set).
    /// </summary>
    public static class Portal
    {
        public const string ViewOwnProfile = "Portal.ViewOwnProfile";
        public const string ViewOwnAppointments = "Portal.ViewOwnAppointments";
        public const string CreateOwnAppointments = "Portal.CreateOwnAppointments";
        public const string CancelOwnAppointments = "Portal.CancelOwnAppointments";
        public const string ViewOwnMedicalRecords = "Portal.ViewOwnMedicalRecords";
        public const string ViewOwnInvoices = "Portal.ViewOwnInvoices";
        public const string PayOwnInvoices = "Portal.PayOwnInvoices";
    }
}
