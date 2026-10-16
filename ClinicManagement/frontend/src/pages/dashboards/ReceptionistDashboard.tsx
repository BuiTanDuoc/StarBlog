import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { StatCard } from "../../components/StatCard";
import { useApiData } from "../../lib/useApiData";

interface ReceptionistDashboardData {
  bookingsToday: number;
  checkInsToday: number;
  waitingPatients: number;
}

export function ReceptionistDashboard() {
  const { t } = useTranslation();
  const { data, isLoading, error } = useApiData<ReceptionistDashboardData>("/api/dashboard/receptionist");

  return (
    <AppShell title={t("dashboard.frontDesk")} roleLabel={t("dashboard.receptionist")}>
      {isLoading && <p className="font-mono text-sm text-ink-muted">{t("dashboard.loadingDashboard")}</p>}
      {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

      {data && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <StatCard label={t("dashboard.newBookingsToday")} value={data.bookingsToday} accent="harbor" />
          <StatCard label={t("dashboard.checkedInToday")} value={data.checkInsToday} accent="success" />
          <StatCard
            label={t("dashboard.waitingRoomNow")}
            value={data.waitingPatients}
            accent={data.waitingPatients > 5 ? "warning" : "gold"}
            caption={data.waitingPatients > 5 ? t("dashboard.busierThanUsual") : undefined}
          />
        </div>
      )}
    </AppShell>
  );
}
