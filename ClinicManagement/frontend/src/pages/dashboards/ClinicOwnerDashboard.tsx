import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { StatCard } from "../../components/StatCard";
import { useApiData } from "../../lib/useApiData";

interface SubscriptionSummary {
  planName: string;
  endDate: string;
  status: string;
  daysRemaining: number;
}

interface ClinicOwnerDashboardData {
  totalPatients: number;
  totalDoctors: number;
  appointmentsToday: number;
  revenueThisMonth: number;
  activeSubscription: SubscriptionSummary | null;
}

const currency = new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" });

export function ClinicOwnerDashboard() {
  const { t } = useTranslation();
  const { data, isLoading, error } = useApiData<ClinicOwnerDashboardData>("/api/dashboard/clinic-owner");

  return (
    <AppShell title={t("nav.overview")} roleLabel={t("dashboard.clinicOwner")}>
      {isLoading && <p className="font-mono text-sm text-ink-muted">{t("dashboard.loadingDashboard")}</p>}
      {error && <p className="rounded-sm bg-danger-light px-4 py-3 text-sm text-danger">{error}</p>}

      {data && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard label={t("dashboard.totalPatients")} value={data.totalPatients} />
            <StatCard label={t("dashboard.totalDoctors")} value={data.totalDoctors} accent="gold" />
            <StatCard label={t("dashboard.appointmentsToday")} value={data.appointmentsToday} accent="success" />
            <StatCard label={t("dashboard.revenueThisMonth")} value={currency.format(data.revenueThisMonth)} accent="harbor" />
          </div>

          <div className="rounded-lg border border-line bg-surface p-6 shadow-card">
            <p className="text-xs font-medium uppercase tracking-[0.1em] text-ink-faint">{t("dashboard.subscription")}</p>
            {data.activeSubscription ? (
              <div className="mt-3 flex items-baseline justify-between">
                <div>
                  <p className="font-display text-2xl text-ink">{data.activeSubscription.planName}</p>
                  <p className="mt-1 text-sm text-ink-muted">
                    {t("common.status")}: <span className="font-medium text-ink">{data.activeSubscription.status}</span>
                  </p>
                </div>
                <div className="text-end">
                  <p className="font-mono text-2xl tabular-nums text-harbor-500">
                    {data.activeSubscription.daysRemaining}
                  </p>
                  <p className="text-xs text-ink-faint">{t("dashboard.daysRemaining")}</p>
                </div>
              </div>
            ) : (
              <p className="mt-3 text-sm text-ink-muted">
                {t("dashboard.noActiveSubscription")}
              </p>
            )}
          </div>
        </div>
      )}
    </AppShell>
  );
}
