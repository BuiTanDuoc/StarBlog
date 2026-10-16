namespace ClinicManagement.Shared.Constants;

/// <summary>
/// Defines which fine-grained Permissions each Role grants. TokenService
/// (Infrastructure, Phase 4) unions this across all of a user's roles and
/// bakes the result into "permission" claims on the JWT at login — so
/// authorization checks at request time are just a claims lookup, no DB
/// round-trip. If permissions need to change for a role, users must log in
/// again (or you add a shorter access-token lifetime / force-refresh flow).
/// </summary>
public static class RolePermissions
{
    public static readonly IReadOnlyDictionary<string, string[]> Map = new Dictionary<string, string[]>
    {
        [Roles.SuperAdmin] = Array.Empty<string>(), // SuperAdmin bypasses permission checks entirely — see PermissionAuthorizationHandler

        [Roles.ClinicOwner] = new[]
        {
            Permissions.Patients.View, Permissions.Patients.Create, Permissions.Patients.Edit, Permissions.Patients.Delete,
            Permissions.Appointments.View, Permissions.Appointments.Create, Permissions.Appointments.Edit, Permissions.Appointments.Cancel,
            Permissions.Billing.View, Permissions.Billing.ManageInvoices, Permissions.Billing.ManagePayments, Permissions.Billing.ManageExpenses,
            Permissions.Billing.ManageTreatmentPackages, Permissions.Billing.SellTreatmentPackages,
            Permissions.MedicalRecords.View, Permissions.MedicalRecords.Create, Permissions.MedicalRecords.Edit, Permissions.MedicalRecords.ManageTemplates,
            Permissions.Tenants.ManageSubscriptions,
            Permissions.Reports.View, Permissions.Reports.Export,
        },

        [Roles.Doctor] = new[]
        {
            Permissions.Patients.View,
            Permissions.Appointments.View, Permissions.Appointments.Edit, Permissions.Appointments.Cancel,
            Permissions.MedicalRecords.View, Permissions.MedicalRecords.Create, Permissions.MedicalRecords.Edit,
            Permissions.Reports.View,
        },

        [Roles.Nurse] = new[]
        {
            Permissions.Patients.View,
            Permissions.Appointments.View,
            Permissions.MedicalRecords.View, Permissions.MedicalRecords.Create,
        },

        [Roles.Receptionist] = new[]
        {
            Permissions.Patients.View, Permissions.Patients.Create, Permissions.Patients.Edit,
            Permissions.Appointments.View, Permissions.Appointments.Create, Permissions.Appointments.Edit, Permissions.Appointments.Cancel,
            Permissions.Billing.View, Permissions.Billing.SellTreatmentPackages,
        },

        [Roles.Accountant] = new[]
        {
            Permissions.Billing.View, Permissions.Billing.ManageInvoices, Permissions.Billing.ManagePayments, Permissions.Billing.ManageExpenses,
            Permissions.Reports.View, Permissions.Reports.Export,
        },

        // Intentionally never includes any staff Permissions.* string — see the note on
        // Permissions.Portal for why that separation matters.
        [Roles.Patient] = new[]
        {
            Permissions.Portal.ViewOwnProfile,
            Permissions.Portal.ViewOwnAppointments,
            Permissions.Portal.CreateOwnAppointments,
            Permissions.Portal.CancelOwnAppointments,
            Permissions.Portal.ViewOwnMedicalRecords,
            Permissions.Portal.ViewOwnInvoices,
            Permissions.Portal.PayOwnInvoices,
        },
    };

    public static string[] GetPermissionsForRoles(IEnumerable<string> roles) =>
        roles.Where(Map.ContainsKey)
            .SelectMany(r => Map[r])
            .Distinct()
            .ToArray();
}
