import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { useApiData } from "../../lib/useApiData";
import { translateAppointmentStatus, formatTime as fmtTime } from "../../lib/format";

interface AppointmentDto {
  id: string;
  patientName: string;
  scheduledStart: string;
  scheduledEnd: string;
  status: string;
  branchName: string;
}

interface DoctorDashboardData {
  todaysAppointments: AppointmentDto[];
  upcomingAppointments: AppointmentDto[];
}

const STATUS_STYLES: Record<string, string> = {
  Pending: "bg-warning-light text-warning",
  Confirmed: "bg-harbor-50 text-harbor-500",
  CheckedIn: "bg-gold-light text-gold-dark",
  InProgress: "bg-gold-light text-gold-dark",
  Completed: "bg-success-light text-success",
  Cancelled: "bg-danger-light text-danger",
  NoShow: "bg-danger-light text-danger",
};

function AppointmentRow({ appointment }: { appointment: AppointmentDto }) {
  const { t } = useTranslation();
  return (
    <div className="flex items-center justify-between border-b border-line py-3.5 last:border-0">
      <div className="flex items-center gap-4">
        <span className="w-16 font-mono text-sm tabular-nums text-ink-muted">
          {fmtTime(appointment.scheduledStart)}
        </span>
        <div>
          <p className="text-sm font-medium text-ink">{appointment.patientName}</p>
          <p className="text-xs text-ink-faint">{appointment.branchName}</p>
        </div>
      </div>
      <span
        className={`rounded-full px-2.5 py-1 text-xs font-medium ${STATUS_STYLES[appointment.status] ?? "bg-line text-ink-muted"}`}
      >
        {translateAppointmentStatus(appointment.status, t)}
      </span>
    </div>
  );
}

export function DoctorDashboard() {
  const { t } = useTranslation();
  const { data, isLoading, error } = useApiData<DoctorDashboardData>("/api/dashboard/doctor");

  return (
    <AppShell title={t("dashboard.todaysSchedule")} roleLabel={t("dashboard.doctor")}>
      {isLoading && <p className="font-mono text-sm text-ink-muted">{t("dashboard.loadingDashboard")}</p>}
      {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

      {data && (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div className="rounded-lg border border-line bg-surface p-6 shadow-card">
            <div className="flex items-baseline justify-between">
              <p className="font-display text-xl text-ink">{t("dashboard.today")}</p>
              <span className="font-mono text-xs text-ink-faint">{t("dashboard.appointmentsCount", { count: data.todaysAppointments.length })}</span>
            </div>
            <div className="mt-3">
              {data.todaysAppointments.length === 0 ? (
                <p className="py-6 text-center text-sm text-ink-faint">{t("dashboard.nothingScheduledToday")}</p>
              ) : (
                data.todaysAppointments.map((a) => <AppointmentRow key={a.id} appointment={a} />)
              )}
            </div>
          </div>

          <div className="rounded-lg border border-line bg-surface p-6 shadow-card">
            <div className="flex items-baseline justify-between">
              <p className="font-display text-xl text-ink">{t("dashboard.upcoming")}</p>
              <span className="font-mono text-xs text-ink-faint">{t("dashboard.next7Days")}</span>
            </div>
            <div className="mt-3">
              {data.upcomingAppointments.length === 0 ? (
                <p className="py-6 text-center text-sm text-ink-faint">{t("dashboard.noUpcomingAppointments")}</p>
              ) : (
                data.upcomingAppointments.map((a) => <AppointmentRow key={a.id} appointment={a} />)
              )}
            </div>
          </div>
        </div>
      )}
    </AppShell>
  );
}
