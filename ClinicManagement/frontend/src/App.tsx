import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth, primaryRouteForRoles } from "./lib/auth";
import { RoleRoute } from "./components/RoleRoute";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { WelcomePage } from "./pages/WelcomePage";
import { ClinicOwnerDashboard } from "./pages/dashboards/ClinicOwnerDashboard";
import { DoctorDashboard } from "./pages/dashboards/DoctorDashboard";
import { ReceptionistDashboard } from "./pages/dashboards/ReceptionistDashboard";
import { PatientsPage } from "./pages/patients/PatientsPage";
import { AppointmentsPage } from "./pages/appointments/AppointmentsPage";
import { DoctorsPage } from "./pages/doctors/DoctorsPage";
import { BranchesPage } from "./pages/branches/BranchesPage";
import { DepartmentsPage } from "./pages/departments/DepartmentsPage";
import { EmployeesPage } from "./pages/employees/EmployeesPage";
import { InvoicesPage } from "./pages/invoices/InvoicesPage";
import { ExpensesPage } from "./pages/expenses/ExpensesPage";
import { PatientChartPage } from "./pages/medical-records/PatientChartPage";
import { MedicalRecordTemplatesPage } from "./pages/medical-record-templates/MedicalRecordTemplatesPage";
import { TreatmentPackagesPage } from "./pages/treatment-packages/TreatmentPackagesPage";
import { CheckoutResultPage } from "./pages/billing/CheckoutResultPage";
import { ReportsPage } from "./pages/reports/ReportsPage";
import { SubscriptionPage } from "./pages/subscription/SubscriptionPage";
import { AuditLogsPage } from "./pages/audit-logs/AuditLogsPage";
import { PlansPage } from "./pages/plans/PlansPage";
import { TeamPage } from "./pages/team/TeamPage";
import { AccountPage } from "./pages/account/AccountPage";
import { PortalRegisterPage } from "./pages/portal/PortalRegisterPage";
import { PortalOverviewPage } from "./pages/portal/PortalOverviewPage";
import { PortalBookAppointmentPage } from "./pages/portal/PortalBookAppointmentPage";
import { PortalAppointmentsPage } from "./pages/portal/PortalAppointmentsPage";
import { PortalMedicalRecordsPage } from "./pages/portal/PortalMedicalRecordsPage";
import { PortalInvoicesPage } from "./pages/portal/PortalInvoicesPage";
import { PortalProfilePage } from "./pages/portal/PortalProfilePage";

/** Roles that hold at least view access to Patients/Appointments per RolePermissions — Accountant is excluded. */
const CLINICAL_STAFF_ROLES = ["ClinicOwner", "Doctor", "Receptionist", "Nurse", "SuperAdmin"];

/** Roles that hold Billing.View per RolePermissions — Doctor/Nurse are excluded. */
const BILLING_VIEW_ROLES = ["ClinicOwner", "Receptionist", "Accountant", "SuperAdmin"];

/** Roles that hold Billing.ManageExpenses per RolePermissions — the Expenses controller itself is gated the same way. */
const EXPENSES_ROLES = ["ClinicOwner", "Accountant", "SuperAdmin"];

/** Roles that hold MedicalRecords.View per RolePermissions — Receptionist/Accountant are excluded. */
const MEDICAL_STAFF_ROLES = ["ClinicOwner", "Doctor", "Nurse", "SuperAdmin"];

/** Roles that hold Reports.View per RolePermissions — Nurse/Receptionist are excluded. */
const REPORTS_ROLES = ["ClinicOwner", "Doctor", "Accountant", "SuperAdmin"];

/** ClinicOwner manages the tenant's own subscription; SuperAdmin oversees everything; AuditLogs is the same set at the controller level. */
const OWNER_ADMIN_ROLES = ["ClinicOwner", "SuperAdmin"];

/** Tenants.Manage (creating/editing platform-wide plans) is a SuperAdmin-only permission — no tenant role holds it. */
const SUPER_ADMIN_ROLES = ["SuperAdmin"];

function RootRedirect() {
  const { user, isLoading } = useAuth();
  if (isLoading) return null;
  return <Navigate to={user ? primaryRouteForRoles(user.roles) : "/login"} replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<RootRedirect />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/portal/register/:slug" element={<PortalRegisterPage />} />
          <Route path="/welcome" element={<RoleRoute><WelcomePage /></RoleRoute>} />

          <Route
            path="/portal"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalOverviewPage />
              </RoleRoute>
            }
          />
          <Route
            path="/portal/book"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalBookAppointmentPage />
              </RoleRoute>
            }
          />
          <Route
            path="/portal/appointments"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalAppointmentsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/portal/medical-records"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalMedicalRecordsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/portal/invoices"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalInvoicesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/portal/profile"
            element={
              <RoleRoute allowedRoles={["Patient"]}>
                <PortalProfilePage />
              </RoleRoute>
            }
          />

          <Route
            path="/dashboard/owner"
            element={
              <RoleRoute allowedRoles={["ClinicOwner"]}>
                <ClinicOwnerDashboard />
              </RoleRoute>
            }
          />
          <Route
            path="/dashboard/doctor"
            element={
              <RoleRoute allowedRoles={["Doctor"]}>
                <DoctorDashboard />
              </RoleRoute>
            }
          />
          <Route
            path="/dashboard/receptionist"
            element={
              <RoleRoute allowedRoles={["Receptionist", "ClinicOwner"]}>
                <ReceptionistDashboard />
              </RoleRoute>
            }
          />

          <Route
            path="/patients"
            element={
              <RoleRoute allowedRoles={CLINICAL_STAFF_ROLES}>
                <PatientsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/appointments"
            element={
              <RoleRoute allowedRoles={CLINICAL_STAFF_ROLES}>
                <AppointmentsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/doctors"
            element={
              <RoleRoute>
                <DoctorsPage />
              </RoleRoute>
            }
          />

          <Route
            path="/branches"
            element={
              <RoleRoute allowedRoles={CLINICAL_STAFF_ROLES}>
                <BranchesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/departments"
            element={
              <RoleRoute allowedRoles={CLINICAL_STAFF_ROLES}>
                <DepartmentsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/employees"
            element={
              <RoleRoute allowedRoles={["ClinicOwner", "SuperAdmin"]}>
                <EmployeesPage />
              </RoleRoute>
            }
          />

          <Route
            path="/invoices"
            element={
              <RoleRoute allowedRoles={BILLING_VIEW_ROLES}>
                <InvoicesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/expenses"
            element={
              <RoleRoute allowedRoles={EXPENSES_ROLES}>
                <ExpensesPage />
              </RoleRoute>
            }
          />

          <Route
            path="/patients/:patientId/chart"
            element={
              <RoleRoute allowedRoles={MEDICAL_STAFF_ROLES}>
                <PatientChartPage />
              </RoleRoute>
            }
          />
          <Route
            path="/medical-record-templates"
            element={
              <RoleRoute allowedRoles={["ClinicOwner", "SuperAdmin"]}>
                <MedicalRecordTemplatesPage />
              </RoleRoute>
            }
          />
          <Route
            path="/treatment-packages"
            element={
              <RoleRoute allowedRoles={["ClinicOwner", "SuperAdmin"]}>
                <TreatmentPackagesPage />
              </RoleRoute>
            }
          />
          <Route path="/billing/checkout-success" element={<CheckoutResultPage outcome="success" />} />
          <Route path="/billing/checkout-cancelled" element={<CheckoutResultPage outcome="cancelled" />} />

          <Route
            path="/reports"
            element={
              <RoleRoute allowedRoles={REPORTS_ROLES}>
                <ReportsPage />
              </RoleRoute>
            }
          />
          <Route
            path="/subscription"
            element={
              <RoleRoute allowedRoles={OWNER_ADMIN_ROLES}>
                <SubscriptionPage />
              </RoleRoute>
            }
          />
          <Route
            path="/audit-logs"
            element={
              <RoleRoute allowedRoles={OWNER_ADMIN_ROLES}>
                <AuditLogsPage />
              </RoleRoute>
            }
          />

          <Route
            path="/plans"
            element={
              <RoleRoute allowedRoles={SUPER_ADMIN_ROLES}>
                <PlansPage />
              </RoleRoute>
            }
          />

          <Route
            path="/team"
            element={
              <RoleRoute allowedRoles={["ClinicOwner"]}>
                <TeamPage />
              </RoleRoute>
            }
          />
          <Route
            path="/account"
            element={
              <RoleRoute>
                <AccountPage />
              </RoleRoute>
            }
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
